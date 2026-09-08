import { useState } from 'react';
import { styles } from '../styles';

interface AuthFormProps {
  onAuthSuccess: (token: string, userRole: string, userId: string) => void;
}

const API_URL = window.location.hostname === 'localhost' 
  ? 'http://localhost:5080/api' 
  : '/api';
  
export function AuthForm({ onAuthSuccess }: AuthFormProps) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [email, setEmail] = useState('');
  const [authError, setAuthError] = useState('');
  const [isSignUp, setIsSignUp] = useState(false);

  // Decode JWT and extract claims
  const executeLoginFlow = (accessToken: string) => {
    const parts = accessToken.split('.');
    const payload = JSON.parse(
      atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'))
    );

    const role = String(payload.roles || payload.role || 'regular user').toLowerCase();
    const userId = payload.uid || payload.sub || '';

    // Store in localStorage for persistence
    localStorage.setItem('token', accessToken);
    localStorage.setItem('userRole', role);
    localStorage.setItem('userId', userId);

    // Notify parent component of successful auth
    onAuthSuccess(accessToken, role, userId);
  };

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setAuthError('');
    try {
      const res = await fetch(`${API_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }),
      });

      if (!res.ok) {
        const errData = await res.json().catch(() => ({}));
        throw new Error(
          errData.detail || errData.title || 'Invalid username or password.'
        );
      }

      const data = await res.json();
      executeLoginFlow(data.accessToken);
    } catch (err: any) {
      setAuthError(err.message);
    }
  };

  const handleSignUp = async (e: React.FormEvent) => {
    e.preventDefault();
    setAuthError('');
    try {
      const res = await fetch(`${API_URL}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, email, password }),
      });

      if (!res.ok) {
        const errData = await res.json().catch(() => ({}));

        if (errData.errors) {
          const errorMessages = Object.values(errData.errors).flat().join(' ');
          throw new Error(errorMessages);
        }

        throw new Error(
          errData.detail || errData.title || 'Registration failed.'
        );
      }

      // Auto-login after successful registration
      const loginRes = await fetch(`${API_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }),
      });

      if (!loginRes.ok)
        throw new Error(
          'Registered successfully, but auto-login failed. Sign in manually.'
        );
      const data = await loginRes.json();

      executeLoginFlow(data.accessToken);
    } catch (err: any) {
      setAuthError(err.message);
    }
  };

  const handleSubmit = isSignUp ? handleSignUp : handleLogin;
  const handleToggleMode = () => {
    setIsSignUp(!isSignUp);
    setAuthError('');
  };

  return (
    <div style={styles.authContainer} className="card">
      <h2 style={styles.authHeading}>
        {isSignUp ? 'Create MeetingRoom Account' : 'MeetingRoom Sign In'}
      </h2>
      {authError && <p style={styles.authError}>{authError}</p>}
      <form onSubmit={handleSubmit}>
        <label style={styles.authLabel}>Username</label>
        <input
          type="text"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
        />
        {isSignUp && (
          <>
            <label style={styles.authLabel}>Email Address</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required={isSignUp}
            />
          </>
        )}

        <label style={styles.authLabel}>Password</label>
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />

        <button type="submit" style={styles.authButton}>
          {isSignUp ? 'Sign Up (Register)' : 'Sign In'}
        </button>
      </form>
      <p style={styles.authToggleText}>
        {isSignUp ? 'Already have an account?' : "Don't have an account?"}{' '}
        <span style={styles.authToggleLink} onClick={handleToggleMode}>
          {isSignUp ? 'Sign In here' : 'Register/Sign Up here'}
        </span>
      </p>
    </div>
  );
}
