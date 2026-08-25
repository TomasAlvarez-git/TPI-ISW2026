import { instance } from '../../shared/api/axiosInstance';

export const createProduct = async (formData) => {
  console.log("Enviando producto:", formData);

  return await instance.post('/api/products', {
    sku: formData.sku,
    internalCode: formData.internalCode,
    name: formData.name,
    description: formData.description,
    currentUnitPrice: formData.currentUnitPrice,
    stockQuantity: formData.stockQuantity,
  });
};