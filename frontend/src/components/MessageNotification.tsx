import { styles } from '../styles';

interface MessageNotificationProps {
  message: string;
}

export function MessageNotification({ message }: MessageNotificationProps) {
  if (!message) return null;

  return (
    <div className="card" style={styles.messageNotification}>
      {message}
    </div>
  );
}
