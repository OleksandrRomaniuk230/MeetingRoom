import { useState, useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import type { AdminPanelHandle } from './components';
import {
  AuthForm,
  Header,
  AdminPanel,
  AuditLog,
  RoomGrid,
  MessageNotification,
} from './components';

interface TimeSlot {
  id: string;
  startTime: string;
  endTime: string;
  isBooked: boolean;
  bookedByUserId?: string;
  bookedByUsername?: string;
}

interface Room {
  id: string;
  name: string;
  capacity: number;
  slots?: TimeSlot[];
}

const API_URL = 'http://localhost:5080/api';

export default function App() {
  // ========== Global Authentication State ==========
  const [token, setToken] = useState<string | null>(
    localStorage.getItem('token')
  );
  const [userRole, setUserRole] = useState<string | null>(
    localStorage.getItem('userRole')
  );
  const [currentUserId, setCurrentUserId] = useState<string | null>(
    localStorage.getItem('userId')
  );

  // ========== Global Data State ==========
  const [rooms, setRooms] = useState<Room[]>([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [showAllBookings, setShowAllBookings] = useState(false);

  // ========== Refs for Component Communication ==========
  const adminPanelRef = useRef<AdminPanelHandle>(null);

  // ========== Effects & Handlers ==========

  // Fetch all rooms and their slots
  const fetchRoomsAndSlots = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${API_URL}/rooms`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to load meeting rooms.');
      const roomsList: Room[] = await res.json();

      const completedData = await Promise.all(
        roomsList.map(async (room) => {
          const slotRes = await fetch(`${API_URL}/rooms/${room.id}/slots`, {
            headers: { Authorization: `Bearer ${token}` },
          });
          const slots = slotRes.ok ? await slotRes.json() : [];
          return { ...room, slots };
        })
      );
      setRooms(completedData);
    } catch (err: any) {
      setMessage(err.message);
    } finally {
      setLoading(false);
    }
  };

  // Initial data fetch when authenticated
  useEffect(() => {
    if (token) {
      fetchRoomsAndSlots();
    }
  }, [token]);

  // Setup SignalR connection for real-time updates
  useEffect(() => {
    if (!token) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5080/api/hubs/bookings', {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    connection.on('SlotStatusChanged', (slotId: string, isBooked: boolean) => {
      setRooms((prevRooms) =>
        prevRooms.map((room) => ({
          ...room,
          slots: room.slots?.map((slot) =>
            slot.id === slotId ? { ...slot, isBooked: isBooked } : slot
          ),
        }))
      );
    });

    connection
      .start()
      .catch((err) => console.error('SignalR error: ', err));

    return () => {
      connection.stop();
    };
  }, [token]);

  // Handle successful authentication
  const handleAuthSuccess = (
    accessToken: string,
    role: string,
    userId: string
  ) => {
    setToken(accessToken);
    setUserRole(role);
    setCurrentUserId(userId);
  };

  // Handle logout
  const handleLogout = () => {
    localStorage.clear();
    setToken(null);
    setUserRole(null);
    setCurrentUserId(null);
    setRooms([]);
    setShowAllBookings(false);
  };

  const isAdmin = userRole === 'admin';

  // ========== Render Logic ==========

  // Unauthenticated: show auth form
  if (!token) {
    return <AuthForm onAuthSuccess={handleAuthSuccess} />;
  }

  // Authenticated: show dashboard
  return (
    <div>
      <Header
        userRole={userRole}
        isAdmin={isAdmin}
        showAllBookings={showAllBookings}
        onToggleAudit={() => setShowAllBookings(!showAllBookings)}
        onLogout={handleLogout}
      />

      <MessageNotification message={message} />

      <AdminPanel
        ref={adminPanelRef}
        isAdmin={isAdmin}
        showAllBookings={showAllBookings}
        token={token}
        onMessage={setMessage}
        onRefresh={fetchRoomsAndSlots}
      />

      {isAdmin && showAllBookings ? (
        <AuditLog rooms={rooms} />
      ) : (
        <RoomGrid
          rooms={rooms}
          isAdmin={isAdmin}
          currentUserId={currentUserId}
          token={token}
          loading={loading}
          onEditRoom={(roomId, roomName, capacity) => {
            adminPanelRef.current?.startEdit(roomId, roomName, capacity);
          }}
          onDeleteRoom={(roomId) => {
            adminPanelRef.current?.handleDeleteRoom(roomId);
          }}
          onMessage={setMessage}
          onRefresh={fetchRoomsAndSlots}
        />
      )}
    </div>
  );
}
