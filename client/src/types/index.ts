export type SeatStatus = 'Available' | 'Reserved' | 'Sold';

export interface Seat {
    seatId: string;
    row: string;
    number: number;
    status: SeatStatus;
    reserverId?: string;
}

export interface Showtime {
    id: string;
    movieTitle: string;
    startTime: string;
    seats: Seat[];
}

export interface BookingRequest {
    showtimeId: string;
    seatIds: string[];
    userId: string;
}