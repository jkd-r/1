import React, { useCallback } from 'react'
import { HistoryEntry } from '../types'
import './HistoryPanel.css'

interface HistoryPanelProps {
  history: HistoryEntry[]
  onSelectEntry: (entry: HistoryEntry) => void
  onClearHistory: () => void
}

const formatDate = (timestamp: number): string => {
  const date = new Date(timestamp)
  const now = new Date()
  const diff = now.getTime() - date.getTime()

  // Within last minute
  if (diff < 60000) {
    return 'Just now'
  }

  // Within last hour
  if (diff < 3600000) {
    const minutes = Math.floor(diff / 60000)
    return `${minutes}m ago`
  }

  // Within last day
  if (diff < 86400000) {
    const hours = Math.floor(diff / 3600000)
    return `${hours}h ago`
  }

  // Format as date
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export const HistoryPanel: React.FC<HistoryPanelProps> = ({
  history,
  onSelectEntry,
  onClearHistory,
}) => {
  const handleSelectEntry = useCallback(
    (entry: HistoryEntry) => {
      onSelectEntry(entry)
    },
    [onSelectEntry]
  )

  return (
    <div className="history-panel">
      <div className="history-header">
        <h3>Recent History</h3>
        <button
          className="clear-history-btn"
          title="Clear all history"
          onClick={onClearHistory}
          disabled={history.length === 0}
        >
          Clear
        </button>
      </div>

      <div className="history-list">
        {history.length === 0 ? (
          <div className="history-empty">
            <p>No history yet</p>
            <small>Your code generations will appear here</small>
          </div>
        ) : (
          history.map((entry) => (
            <button
              key={entry.id}
              className="history-item"
              onClick={() => handleSelectEntry(entry)}
              title={`${entry.files.length} file(s) - ${entry.designInputs.componentName}`}
            >
              <div className="history-item-content">
                <div className="history-item-name">{entry.designInputs.componentName}</div>
                <div className="history-item-meta">
                  {entry.files.length} file{entry.files.length !== 1 ? 's' : ''}
                </div>
              </div>
              <div className="history-item-time">{formatDate(entry.timestamp)}</div>
            </button>
          ))
        )}
      </div>
    </div>
  )
}
