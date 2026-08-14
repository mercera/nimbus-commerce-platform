import styles from './FormMessages.module.css';

interface FormMessagesProps {
  messages: string[];
  variant?: 'error' | 'success';
}

export function FormMessages({ messages, variant = 'error' }: FormMessagesProps) {
  if (messages.length === 0) return null;

  const variantClass = variant === 'error' ? styles.error : styles.success;

  return (
    <ul className={[styles.messages, variantClass].join(' ')} role={variant === 'error' ? 'alert' : 'status'}>
      {messages.map((message) => (
        <li key={message}>{message}</li>
      ))}
    </ul>
  );
}
