
import AnimatedCard from './AnimatedCard';
import StructuredCard from './StructuredCard';
import ResponsiveText from './ResponsiveText';
import Button from './Button';

function ListCard({ keyProp, openId, title, closedContent, openedContent, handleClicked }) {

    const isOpen = openId === keyProp;

    return (
        <>
            <AnimatedCard key={keyProp}>
                <StructuredCard
                    key={keyProp}
                    className='flex flex-row justify-between hover:bg-gray-50 items-center p-4'
                    titleClassName='font-semibold text-xl'
                    title={title}

                    content={
                        <>
                            <ResponsiveText as='p' className='text-gray-700'>
                                {closedContent}
                            </ResponsiveText>

                            {isOpen && (
                                <div className='flex flex-col mt-2.5 pt-2 border-t border-gray-300'>
                                    <ResponsiveText as="ol" className='space-y-1'>
                                        {openedContent}
                                    </ResponsiveText>
                                </div>
                            )}
                        </>
                    }
                    actions={
                        <>
                            <Button
                                onClick={() => handleClicked(keyProp)}
                            >
                                {isOpen ? 'Ocultar' : 'Ver'}
                            </Button>
                        </>
                    }
                />
            </AnimatedCard>
        </>
    );
}
export default ListCard;