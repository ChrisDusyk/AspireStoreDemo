interface ProductImageProps {
  imageUrl: string | null;
  alt: string;
  className?: string;
}

function ProductImage({ imageUrl, alt, className = '' }: ProductImageProps) {
  const imageSrc = imageUrl || '/product-placeholder.svg';

  return (
    <img
      src={imageSrc}
      alt={alt}
      className={className}
      onError={(e) => {
        // Fallback to placeholder if image fails to load
        e.currentTarget.src = '/product-placeholder.svg';
      }}
    />
  );
}

export default ProductImage;
