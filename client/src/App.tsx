import { useEffect, useState } from 'react';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { api } from './api/client';
import type { Showtime, Seat as SeatType } from './types';
import { Seat } from './components/Seat';
import { CinemaScreen } from './components/CinemaScreen';
import { BookingSummary } from './components/BookingSummary';
import { Loader2, Armchair } from 'lucide-react';

// Constants
const SHOWTIME_ID = "11111111-1111-1111-1111-111111111111";
const USER_ID = "3fa85f64-5717-4562-b3fc-2c963f66afa6"; // Mock User
const SEAT_PRICE = 15;

function App() {
  const [showtime, setShowtime] = useState<Showtime | null>(null);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [connection, setConnection] = useState<HubConnection | null>(null);

  // 1. Initialize Data & SignalR
  useEffect(() => {
    const init = async () => {
      try {
        // Fetch Initial State
        const { data } = await api.get<Showtime>(`/showtimes/${SHOWTIME_ID}`);
        setShowtime(data);

        // Connect SignalR
        const newConnection = new HubConnectionBuilder()
          .withUrl("http://localhost:5000/hub/bookings")
          .withAutomaticReconnect()
          .build();

        newConnection.on("ReceiveSeatUpdate", (update: { seatId: string, status: string }) => {
          console.log("Live Update:", update);

          setShowtime(prev => {
            if (!prev) return null;
            return {
              ...prev,
              seats: prev.seats.map(s =>
                s.seatId === update.seatId ? { ...s, status: update.status as any } : s
              )
            };
          });

          // If someone else booked a seat I had selected, deselect it
          if (update.status === 'Reserved') {
            setSelectedIds(prev => prev.filter(id => id !== update.seatId));
          }
        });

        await newConnection.start();
        await newConnection.invoke("JoinShowtime", SHOWTIME_ID);
        setConnection(newConnection);

      } catch (err) {
        console.error("Initialization failed", err);
      }
    };

    init();

    return () => {
      connection?.stop();
    };
  }, []);

  // 2. Handle Selection
  const toggleSeat = (id: string) => {
    setSelectedIds(prev =>
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
    );
  };

  // 3. Handle Booking
  const handleBooking = async () => {
    if (selectedIds.length === 0) return;
    setIsProcessing(true);

    try {
      await api.post('/bookings', {
        showtimeId: SHOWTIME_ID,
        seatIds: selectedIds,
        userId: USER_ID
      });

      // Success! Clear selection. 
      // We wait for SignalR to update the visual state to "Reserved"
      setSelectedIds([]);

    } catch (error: any) {
      alert("Booking Failed: " + (error.response?.data?.message || "Unknown error"));
      // If failed, we should probably refresh the map to see what happened
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

  // Group seats by Row
  const rows = [...new Set(showtime.seats.map(s => s.row))].sort();

  return (
    <div className="min-h-screen bg-cinema-900 text-white p-8 pb-32 overflow-x-hidden">

      {/* Header */}
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

      {/* Cinema Layout */}
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
                      {...seat}
                      isSelected={selectedIds.includes(seat.seatId)}
                      onClick={() => toggleSeat(seat.seatId)}
                    />
                  ))}
              </div>
              <span className="w-4 text-gray-500 font-mono text-sm text-right">{rowName}</span>
            </div>
          ))}
        </div>
      </main>

      {/* Floating Action Bar */}
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