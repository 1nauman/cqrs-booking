import { useEffect, useState } from 'react';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { api } from './api/client';
import type { Showtime } from './types';
import { Seat } from './components/Seat';
import { CinemaScreen } from './components/CinemaScreen';
import { BookingSummary } from './components/BookingSummary';
import { SuccessModal } from './components/SuccessModal'; // Import the modal
import { Loader2 } from 'lucide-react';

const SHOWTIME_ID = "11111111-1111-1111-1111-111111111111";
const USER_ID = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
const SEAT_PRICE = 15;

function App() {
  const [showtime, setShowtime] = useState<Showtime | null>(null);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false); // Success State
  const [connection, setConnection] = useState<HubConnection | null>(null);

  // Dragging State
  const [isDragging, setIsDragging] = useState(false);

  useEffect(() => {
    const init = async () => {
      try {
        const { data } = await api.get<Showtime>(`/showtimes/${SHOWTIME_ID}`);
        setShowtime(data);

        const newConnection = new HubConnectionBuilder()
          .withUrl("http://localhost:8080/hub/bookings")
          .withAutomaticReconnect()
          .build();

        newConnection.on("SeatsStatusChanged", (event: { seatIds: string[], status: string }) => {

          console.log(`[SignalR] Seats ${event.seatIds.join(', ')} are now ${event.status}`);

          setShowtime(prev => {
            if (!prev) return null;
            return {
              ...prev,
              seats: prev.seats.map(seat =>
                event.seatIds.includes(seat.seatId)
                  ? { ...seat, status: event.status as any }
                  : seat
              )
            };
          });

          // Logic to handle "My Selection" invalidation
          if (event.status === 'Reserved' || event.status === 'Sold') {
            setSelectedIds(prev => prev.filter(id => !event.seatIds.includes(id)));
          }
        });

        await newConnection.start();
        await newConnection.invoke("JoinShowtime", SHOWTIME_ID);
        setConnection(newConnection);
      } catch (err) {
        console.error("Init failed", err);
      }
    };

    init();
    return () => { connection?.stop(); };
  }, []);

  // --- DRAG SELECTION LOGIC ---

  // 1. Start Dragging
  const handleMouseDown = (id: string) => {
    setIsDragging(true);
    toggleSeat(id);
  };

  // 2. Drag Over (Paint)
  const handleMouseEnter = (id: string) => {
    if (isDragging) {
      // Only add if not already selected to avoid flickering
      if (!selectedIds.includes(id)) {
        toggleSeat(id);
      }
    }
  };

  // 3. Stop Dragging (Global Mouse Up)
  const handleMouseUp = () => {
    setIsDragging(false);
  };

  // Attach global mouse up listener
  useEffect(() => {
    window.addEventListener('mouseup', handleMouseUp);
    return () => window.removeEventListener('mouseup', handleMouseUp);
  }, []);

  const toggleSeat = (id: string) => {
    setSelectedIds(prev =>
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
    );
  };

  // --- BOOKING LOGIC ---

  const handleBooking = async () => {
    if (selectedIds.length === 0) return;
    setIsProcessing(true);

    try {
      await api.post('/bookings', {
        showtimeId: SHOWTIME_ID,
        seatIds: selectedIds,
        userId: USER_ID
      });

      // --- OPTIMISTIC UPDATE ---
      // Don't wait for SignalR. We know we succeeded.
      // Immediately mark these seats as Reserved in our local view.
      setShowtime(prev => {
        if (!prev) return null;
        return {
          ...prev,
          seats: prev.seats.map(s =>
            selectedIds.includes(s.seatId) ? { ...s, status: 'Reserved' } : s
          )
        };
      });

      // Clear selection and show success
      setSelectedIds([]);
      setShowSuccess(true);

    } catch (error: any) {
      alert("Booking Failed: " + (error.response?.data?.message || "Unknown error"));
      // Refresh data on failure to see what happened
      const { data } = await api.get<Showtime>(`/showtimes/${SHOWTIME_ID}`);
      setShowtime(data);
    } finally {
      setIsProcessing(false);
    }
  };

  if (!showtime) return (
    <div className="min-h-screen flex items-center justify-center text-cinema-500">
      <Loader2 className="w-10 h-10 animate-spin" />
    </div>
  );

  const rows = [...new Set(showtime.seats.map(s => s.row))].sort();

  return (
    <div className="min-h-screen bg-cinema-900 text-white p-8 pb-32 overflow-x-hidden select-none">

      {/* Success Modal */}
      <SuccessModal isOpen={showSuccess} onClose={() => setShowSuccess(false)} />

      <header className="max-w-5xl mx-auto flex justify-between items-center mb-12">
        <div>
          <h1 className="text-3xl font-bold bg-gradient-to-r from-white to-gray-400 bg-clip-text text-transparent">
            {showtime.movieTitle}
          </h1>
          <p className="text-cinema-500 text-sm font-medium mt-1">
            {new Date(showtime.startTime).toLocaleString()}
          </p>
        </div>
        <div className="flex gap-6 text-sm">
          <div className="flex items-center gap-2"><div className="w-4 h-4 rounded bg-cinema-700"></div> Available</div>
          <div className="flex items-center gap-2"><div className="w-4 h-4 rounded bg-cinema-500"></div> Selected</div>
          <div className="flex items-center gap-2"><div className="w-4 h-4 rounded bg-cinema-800 border border-cinema-700"></div> Reserved</div>
        </div>
      </header>

      <main className="max-w-4xl mx-auto perspective-1000">
        <CinemaScreen />

        <div className="flex flex-col gap-3 items-center">
          {rows.map(rowName => (
            <div key={rowName} className="flex items-center gap-8 hover:bg-white/5 p-2 rounded-xl transition-colors duration-300">
              <span className="w-4 text-gray-500 font-mono text-sm">{rowName}</span>
              <div className="flex gap-3">
                {showtime.seats
                  .filter(s => s.row === rowName)
                  .sort((a, b) => a.number - b.number)
                  .map(seat => (
                    <Seat
                      key={seat.seatId}
                      id={seat.seatId} // Pass ID
                      row={seat.row}
                      number={seat.number}
                      status={seat.status}
                      isSelected={selectedIds.includes(seat.seatId)}
                      onMouseDown={handleMouseDown} // Pass Handler
                      onMouseEnter={handleMouseEnter} // Pass Handler
                    />
                  ))}
              </div>
              <span className="w-4 text-gray-500 font-mono text-sm text-right">{rowName}</span>
            </div>
          ))}
        </div>
      </main>

      <BookingSummary
        selectedCount={selectedIds.length}
        price={selectedIds.length * SEAT_PRICE}
        onConfirm={handleBooking}
        isProcessing={isProcessing}
      />
    </div>
  );
}

export default App;