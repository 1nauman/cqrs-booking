export const CinemaScreen = () => {
    return (
        <div className="w-full flex flex-col items-center mb-16">
            {/* The Glowing Screen */}
            <div className="w-[80%] h-16 bg-gradient-to-b from-white/20 to-transparent rounded-[50%] screen-curve animate-screen-glow backdrop-blur-sm border-t border-white/10"></div>
            <p className="text-cinema-600 text-sm mt-8 tracking-[0.5em] font-light">SCREEN</p>
        </div>
    );
};