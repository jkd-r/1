import React from 'react'
import { Toast } from './Toast'
import { ToastMessage } from '../types'
import './ToastContainer.css'

interface ToastContainerProps {
  messages: ToastMessage[]
  onClose: (id: string) => void
}

export const ToastContainer: React.FC<ToastContainerProps> = ({ messages, onClose }) => {
  return (
    <div className="toast-container">
      {messages.map((message) => (
        <Toast key={message.id} message={message} onClose={onClose} />
      ))}
    </div>
  )
}
