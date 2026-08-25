import { useState, useImperativeHandle, forwardRef } from "react";
import Modal from "../../shared/components/Modal";
import ExtraInfoForm from "./ExtraInfoForm";
import SuccessModal from "../../shared/components/SuccessModal";

const OrderManager = forwardRef(({ cartItems, onSuccess }, ref) => {
  const [isOpenInfo, setIsOpenInfo] = useState(false);
  const [isOpenSuccess, setIsOpenSuccess] = useState(false);

  const startOrder = () => {
    setIsOpenInfo(true);
  };

  const handleSuccessInfo = () => {
    setIsOpenInfo(false);
    setIsOpenSuccess(true);
    localStorage.removeItem("cart");
  };

  const handleSuccessConfirm = () => {
    if (onSuccess) {
      onSuccess();
    }
  };

  useImperativeHandle(ref, () => ({
    triggerOrder: startOrder,
  }));

  return (
    <>
      <Modal isOpen={isOpenInfo} onClose={() => setIsOpenInfo(false)}>
        <ExtraInfoForm
          onCancel={() => setIsOpenInfo(false)}
          onSuccess={handleSuccessInfo}
        />
      </Modal>

      <SuccessModal
        isOpen={isOpenSuccess}
        onClose={() => setIsOpenSuccess(false)}
        successText="Orden realizada, muchas gracias!"
        onConfirm={handleSuccessConfirm}
        showConfetti={true}
      />

      <button
        id="hiddenOrder"
        style={{ display: "none" }}
        onClick={startOrder}
      />
    </>
  );
});

OrderManager.displayName = "OrderManager";
export default OrderManager;
