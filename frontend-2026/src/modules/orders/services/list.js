import { instance } from '../../shared/api/axiosInstance';

export const getOrders = async (search = null, status = null, pageNumber = 1, pageSize = 20) => {
    const queryString = new URLSearchParams({
        search,
        pageNumber,
        pageSize,
    });
    if (status) {
        queryString.set('status', status);
    }
    const response = await instance.get(`api/orders?${queryString}`);
    return { dataOrders: response.data, errorOrders: null };
};
