import React, { useEffect, useState } from 'react'
import { ToastMessage } from '../types'
import './Toast.css'

interface ToastProps {
  message: ToastMessage
  onClose: (id: string) => void
}

export const Toast: React.FC<ToastProps> = ({ message, onClose }) => {
  const [fadeOut, setFadeOut] = useState(false)

  useEffect(() => {
    const duration = message.duration || 3000
    const timer = setTimeout(() => {
      setFadeOut(true)
      const removeTimer = setTimeout(() => {
        onClose(message.id)
      }, 300)
      return () => clearTimeout(removeTimer)
    }, duration)

    return () => clearTimeout(timer)
  }, [message, onClose])

  return (
    <div className={`toast ${message.type} ${fadeOut ? 'fadeOut' : ''}`}>
      {message.message}
    </div>
  )
}
