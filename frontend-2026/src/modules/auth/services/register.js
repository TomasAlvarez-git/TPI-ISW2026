import { instance } from '../../shared/api/axiosInstance';

export const register = async ({ username, email, role, password }) => {
    const payload = {
        username,
        email,
        role,
        password
    };

    const response = await instance.post('api/auth/register', payload);

    return { data: response.data, error: null };
};