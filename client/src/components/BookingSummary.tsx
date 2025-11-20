import { Loader2 } from 'lucide-react';

interface Props {
    selectedCount: number;
    price: number;
    onConfirm: () => void;
    isProcessing: boolean;
}

export const BookingSummary = ({ selectedCount, price, onConfirm, isProcessing }: Props) => {
    if (selectedCount === 0) return null;

    return (
        <div className="fixed bottom-8 left-1/2 -translate-x-1/2 bg-cinema-800/90 backdrop-blur-md border border-cinema-700 px-8 py-4 rounded-2xl shadow-2xl flex items-center gap-12 animate-in slide-in-from-bottom-10 fade-in duration-300 z-50">

            <div className="flex flex-col">
                <span className="text-xs text-gray-400 uppercase tracking-wider">Selected</span>
                <span className="text-xl font-bold text-white">{selectedCount} <span className="text-sm font-normal text-gray-500">seats</span></span>
            </div>

            <div className="h-8 w-px bg-cinema-700"></div>

            <div className="flex flex-col">
                <span className="text-xs text-gray-400 uppercase tracking-wider">Total</span>
                <span className="text-xl font-bold text-white">${price}</span>
            </div>

            <button
                onClick={onConfirm}
                disabled={isProcessing}
                className="ml-4 bg-cinema-500 hover:bg-cinema-400 text-white px-8 py-3 rounded-xl font-semibold transition-all active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 shadow-[0_0_20px_rgba(225,29,72,0.3)]"
            >
                {isProcessing && <Loader2 className="w-4 h-4 animate-spin" />}
                {isProcessing ? 'Booking...' : 'Confirm Booking'}
            </button>
        </div>
    );
};