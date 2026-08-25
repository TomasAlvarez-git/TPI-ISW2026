import { instance } from '../../shared/api/axiosInstance';

export const createOrder = async (formData) => {
  console.log("Enviando orden:", formData);

  return await instance.post('/api/orders', {
    shippingAddress: formData.shippingAddress,
    billingAddress: formData.billingAddress,
    notes: formData.notes,
    customerId: formData.customerId,
    orderItems: formData.orderItems,
  });
};