interface ErrorDisplayProps {
  message: string;
}

function ErrorDisplay({ message }: ErrorDisplayProps) {
  return (
    <div
      className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative text-center"
      role="alert"
    >
      <span className="block sm:inline">{message}</span>
    </div>
  );
}

export default ErrorDisplay;
