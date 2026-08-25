import React from 'react';
import Card from './Card';
import ResponsiveText from './ResponsiveText';

function StructuredCard({ title, content, actions, className, titleClassName, contentClassName, actionsClassName, ...rest }) {
  return (
    <Card className={className} {...rest}>
      <div>
        <div className="mb-2">
          <ResponsiveText as="h2" className={`text-2xl font-semibold ${titleClassName || ""}`}>
            {title}
          </ResponsiveText>
        </div>

        <div className={`mb-3 text-lg ${contentClassName || ""}`}>
          {content}
        </div>
      </div>
      <div>
        <div className='mb-2'>
          {actions && (
            <div className={`ml-3 ${actionsClassName || ""}`}>
              {actions}
            </div>
          )}
        </div>
      </div>
    </Card>
  );
};

export default StructuredCard;