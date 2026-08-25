import { useEffect } from "react";
import confetti from "canvas-confetti";
import Modal from "./Modal";
import Button from "./Button";

function SuccessModal({
    isOpen,
    onClose,
    successText = "¡Éxito!",
    onConfirm,
    showConfetti = true
}) {
    const launchConfetti = () => {
        const duration = 2 * 1000;
        const end = Date.now() + duration;

        (function frame() {
            confetti({
                particleCount: 5,
                angle: 60,
                spread: 55,
                origin: { x: 0 },
            });
            confetti({
                particleCount: 5,
                angle: 120,
                spread: 55,
                origin: { x: 1 },
            });

            if (Date.now() < end) {
                requestAnimationFrame(frame);
            }
        })();
    };

    useEffect(() => {
        if (isOpen && showConfetti) {
            launchConfetti();
        }
    }, [isOpen, showConfetti]);

    const handleClose = () => {
        if (onConfirm) {
            onConfirm();
        }
        onClose();
    };

    return (
        <Modal isOpen={isOpen} onClose={onClose}>
            <div className="flex flex-col items-center text-center gap-4 p-5">
                <div className="text-2xl font-bold">{successText}</div>
                <Button onClick={handleClose}>
                    Cerrar
                </Button>
            </div>
        </Modal>
    );
}

export default SuccessModal;
