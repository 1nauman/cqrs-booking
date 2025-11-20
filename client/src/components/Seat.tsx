import { cn } from '../utils/cn'; // Helper we will create below
import type { SeatStatus } from '../types';

interface SeatProps {
    row: string;
    number: number;
    status: SeatStatus;
    isSelected: boolean;
    onClick: () => void;
}

export const Seat = ({ row, number, status, isSelected, onClick }: SeatProps) => {

    const isInteractive = status === 'Available';

    return (
        <div className="relative group flex flex-col items-center gap-1">
            <button
                disabled={!isInteractive}
                onClick={onClick}
                className={cn(
                    "relative w-10 h-10 transition-all duration-300 transform",
                    // Status Styles
                    status === 'Available' && !isSelected && "fill-cinema-700 hover:fill-cinema-600 hover:scale-110 cursor-pointer",
                    status === 'Reserved' && "fill-cinema-800 stroke-cinema-700 cursor-not-allowed opacity-50",
                    status === 'Sold' && "fill-cinema-900 stroke-cinema-800 cursor-not-allowed opacity-30",
                    isSelected && "fill-cinema-500 scale-110 drop-shadow-[0_0_10px_rgba(225,29,72,0.5)]",
                )}
            >
                {/* The Armchair SVG */}
                <svg viewBox="0 0 100 100" className="w-full h-full">
                    {/* Backrest */}
                    <path d="M15,40 Q15,10 50,10 Q85,10 85,40 V80 H75 V50 H25 V80 H15 Z" />
                    {/* Seat Cushion */}
                    <rect x="20" y="45" width="60" height="40" rx="8" />
                    {/* Armrests */}
                    <rect x="10" y="50" width="10" height="35" rx="4" />
                    <rect x="80" y="50" width="10" height="35" rx="4" />
                </svg>
            </button>

            {/* Tooltip on Hover */}
            <span className="absolute -top-8 bg-white text-black text-[10px] font-bold px-2 py-1 rounded opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none whitespace-nowrap z-10">
                {row}-{number}
            </span>
        </div>
    );
};