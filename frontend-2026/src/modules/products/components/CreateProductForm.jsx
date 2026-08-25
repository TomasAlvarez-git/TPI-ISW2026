import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import Button from '../../shared/components/Button';
import Card from '../../shared/components/Card';
import Input from '../../shared/components/Input';
import { createProduct } from '../services/create';
import { useState } from 'react';
import { frontendErrorMessage } from '../helpers/backendError';
import ResponsiveText from '../../shared/components/ResponsiveText';
import SuccessModal from '../../shared/components/SuccessModal';

function CreateProductForm() {
  const {
    register,
    formState: { errors },
    handleSubmit,
  } = useForm({
    mode: "onChange", 
    defaultValues: {
      sku: '',
      cui: '',
      name: '',
      description: '',
      price: 0,
      stock: 0,
    },
  });

  const [errorBackendMessage, setErrorBackendMessage] = useState('');
  const [isOpenSuccess, setIsOpenSuccess] = useState(false);
  const navigate = useNavigate();

  const handleSuccessConfirm = () => {
    navigate('/admin/products');
  };

  const onValid = async (formData) => {
    
    try {
    const payload = {
      sku: `SKU-${formData.sku}`,
      internalCode: `INT-${formData.cui}`,
      name: formData.name,
      description: formData.description,
      currentUnitPrice: Number(formData.price),
      stockQuantity: Number(formData.stock),   
    };
      
      await createProduct(payload);
      setIsOpenSuccess(true);
    } catch (error) {
      if (error.response?.data?.detail) {
        const errorMessage = frontendErrorMessage[error.response.data.code];

        setErrorBackendMessage(errorMessage);
      } else {
        setErrorBackendMessage('Contactar a Soporte');
      }
    }
  };

  return (
    <Card>
      <form
        className='
          flex
          flex-col
          gap-5
          p-8
        '
        onSubmit={handleSubmit(onValid)}
      >
        <Input
          label='SKU'
          error={errors.sku?.message}
          {...register('sku', {
            required: 'SKU es requerido',
            validate: (val) => {
              const str = String(val);
              const regex = /^(0|[1-9]\d*)$/;
              return regex.test(str) || 'Ingrese un SKU válido (solo se permiten numeros)';
            }
          })}
        />
        <Input
          label='Código Único'
          error={errors.cui?.message}
          {...register('cui', {
            required: 'Código Único es requerido',
            validate: (val) => {
              const str = String(val);
              const regex = /^(0|[1-9]\d*)$/;
              return regex.test(str) || 'Ingrese un codigo válido (solo se permiten numeros)';
            }
          })}
        />
        <Input
          label='Nombre'
          error={errors.name?.message}
          {...register('name', {
            required: 'Nombre es requerido',
            validate: (val) => {
              const str = String(val);
              const regex = /^[A-Za-z0-9\s]+$/;
              return regex.test(str) || 'Ingrese un nombre válido (no se permiten acentos ni simbolos)';
            }
          })}
        />
        <Input
          label='Descripción'
          error={errors.description?.message}
          {...register('description', {
            validate: (val) => {
              const str = String(val);
              const regex = /^[a-zA-Z0-9\s,.\-°º#]*$/;
              return regex.test(str) || 'Ingrese una descripcion válida (no se permiten acentos ni simbolos)';
            }
          })}
        />
        <Input
          label='Precio'
          error={errors.price?.message}
          type='decimal'
          {...register('price', {
            required: 'Precio es requerido',
            validate: (val) => {
              const str = String(val);
              const regex = /^[0-9]+(\.[0-9]+)?$/;
              return regex.test(str) || 'Ingrese un número válido (use . como separador decimal)';
            },
            min: {
              value: 0,
              message: 'No puede tener un precio negativo',
            },
          })}
        />
        <Input
          label='Stock'
          error={errors.stock?.message}
          type='number'
          {...register('stock', {
            required: 'Stock es requerido',
            validate: (val) => {
              const str = String(val);
              const regex = /^\d+$/;
              return regex.test(str) || 'Ingrese un número válido';
            },
            min: {
              value: 0,
              message: 'No puede tener un stock negativo',
            },
          })}
        />
        <div className='sm:text-end'>
          <Button type='submit' className='w-full sm:w-fit'>Crear Producto</Button>
        </div>
        {errorBackendMessage && <ResponsiveText as='p' className='text-red-500 text-lg'>{errorBackendMessage}</ResponsiveText>}
        <SuccessModal
          isOpen={isOpenSuccess}
          onClose={() => setIsOpenSuccess(false)}
          successText="Producto creado correctamente"
          onConfirm={handleSuccessConfirm}
          showConfetti={true}
        />
      </form>
    </Card>
  );
};

export default CreateProductForm;
