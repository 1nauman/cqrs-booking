import { CheckCircle, Ticket } from 'lucide-react';

interface Props {
    isOpen: boolean;
    onClose: () => void;
}

export const SuccessModal = ({ isOpen, onClose }: Props) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm animate-in fade-in duration-200">
            <div className="bg-cinema-800 border border-cinema-600 p-8 rounded-2xl shadow-2xl max-w-sm w-full text-center transform transition-all animate-slide-up">

                <div className="w-20 h-20 bg-green-500/20 rounded-full flex items-center justify-center mx-auto mb-6">
                    <CheckCircle className="w-10 h-10 text-green-500" />
                </div>

                <h2 className="text-2xl font-bold text-white mb-2">Booking Confirmed!</h2>
                <p className="text-gray-400 mb-8">Your seats have been successfully reserved. Enjoy the movie!</p>

                <div className="flex justify-center gap-4">
                    <button
                        onClick={onClose}
                        className="bg-cinema-500 hover:bg-cinema-400 text-white px-8 py-3 rounded-xl font-semibold w-full flex items-center justify-center gap-2"
                    >
                        <Ticket className="w-4 h-4" />
                        View Tickets
                    </button>
                </div>
            </div>
        </div>
    );
};