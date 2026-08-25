import { instance } from '../../shared/api/axiosInstance';

export const createOrder = async (payload) => {
  try {
    const response = await instance.post('/api/orders', payload);
    return { data: response.data, error: null };
  } catch (error) {
    console.error('Error createOrder service:', error);
    return { data: null, error };
  }
};