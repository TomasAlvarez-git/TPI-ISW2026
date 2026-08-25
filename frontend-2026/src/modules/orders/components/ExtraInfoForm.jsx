import { useForm } from 'react-hook-form';
import { useState } from 'react';
import Button from "../../shared/components/Button";
import Input from '../../shared/components/Input';
import { frontendErrorMessage } from '../helpers/backendError';
import { createOrder } from "../services/createOrder";

function ExtraInfoForm({ onCancel, isLoading, onSuccess }) {

    const [errorBackendMessage, setErrorBackendMessage] = useState('');

    const {
        register,
        formState: { errors },
        handleSubmit,
    } = useForm({
        mode: "onChange",
        defaultValues: {
            shippingAdress: '',
            billingAdress: '',
            notes: '',
        },
    });

    const handleOrderSubmission = async (extraInfo) => {
        const customerId = localStorage.getItem("customerId");
        const storedCart = JSON.parse(localStorage.getItem("cart")) || [];

        const payload = {
            shippingAddress: extraInfo.shippingAdress,
            billingAddress: extraInfo.billingAdress,
            notes: extraInfo.notes,
            customerId: customerId,
            orderItems: storedCart.map((item) => ({
                productId: item.productId,
                quantity: Number(item.quantity) || 0,
            })),
        };

        try {
            const { data, error } = await createOrder(payload);

            if (error) {
                const code = error?.response?.data?.code;
                if (code && frontendErrorMessage[code]) {
                    setErrorBackendMessage(frontendErrorMessage[code]);
                } else {
                    setErrorBackendMessage("Error inesperado");
                }
                return;
            }

            onSuccess();

        } catch (err) {
            const code = err?.response?.data?.code;

            setErrorBackendMessage(
                frontendErrorMessage[code] || "Ocurrió un error. Llame a soporte."
            );
        }
    };

    return (
        <form className="flex flex-col gap-4"
            onSubmit={handleSubmit(handleOrderSubmission)}>

            <h3 className="text-xl font-bold mb-2">Información de Envío y Facturación</h3>

            <Input
                label='Dirección de envío'
                {...register('shippingAdress', { required: 'Obligatorio' })}
                error={errors.shippingAdress?.message}
            />

            <Input
                label='Dirección de facturación'
                {...register('billingAdress', { required: 'Obligatorio' })}
                error={errors.billingAdress?.message}
            />

            <Input
                label='Notas adicionales'
                {...register('notes')}
                error={errors.notes?.message}
            />

            {errorBackendMessage && (
                <p className="text-red-600 text-center">{errorBackendMessage}</p>
            )}

            <div className="flex justify-end gap-3 pt-2">
                <Button type="button"
                    onClick={onCancel}
                    variant='secondary'
                    disabled={isLoading}>
                    Cancelar
                </Button>

                <Button type="submit"
                    disabled={isLoading}>
                    {isLoading ? 'Procesando...' : 'Realizar pago'}
                </Button>
            </div>
        </form>
    );
}

export default ExtraInfoForm;
