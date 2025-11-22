import React, { useCallback } from 'react'
import { FileContent, LanguageMode } from '../types'
import { copyToClipboard } from '../utils/clipboardUtils'
import './FilePanel.css'

interface FilePanelProps {
  files: FileContent[]
  selectedFileIndex: number
  onSelectFile: (index: number) => void
  onAddFile: (language: LanguageMode) => void
  onDeleteFile: (index: number) => void
  onCopyFile: (index: number) => void
  onShowToast: (message: string, type: 'success' | 'error') => void
}

export const FilePanel: React.FC<FilePanelProps> = ({
  files,
  selectedFileIndex,
  onSelectFile,
  onAddFile,
  onDeleteFile,
  onCopyFile,
  onShowToast,
}) => {
  const handleCopyClick = useCallback(
    async (index: number, e: React.MouseEvent) => {
      e.stopPropagation()
      const success = await copyToClipboard(files[index].content)
      onShowToast(
        success ? `Copied "${files[index].name}" to clipboard` : 'Failed to copy to clipboard',
        success ? 'success' : 'error'
      )
      onCopyFile(index)
    },
    [files, onCopyFile, onShowToast]
  )

  const handleDeleteClick = useCallback(
    (index: number, e: React.MouseEvent) => {
      e.stopPropagation()
      if (files.length > 1) {
        onDeleteFile(index)
      } else {
        onShowToast('Cannot delete the last file', 'error')
      }
    },
    [files.length, onDeleteFile, onShowToast]
  )

  return (
    <div className="file-panel">
      <div className="file-panel-header">
        <h3>Files</h3>
        <button
          className="add-file-btn"
          title="Add new file"
          onClick={() => onAddFile('javascript')}
        >
          + New
        </button>
      </div>

      <div className="file-list">
        {files.map((file, index) => (
          <div
            key={index}
            className={`file-item ${selectedFileIndex === index ? 'active' : ''}`}
            onClick={() => onSelectFile(index)}
            title={file.name}
          >
            <span className="file-icon">📄</span>
            <span className="file-name">{file.name}</span>
            <div className="file-actions">
              <button
                className="copy-btn"
                title="Copy file content"
                onClick={(e) => handleCopyClick(index, e)}
              >
                📋
              </button>
              <button
                className="delete-btn"
                title="Delete file"
                onClick={(e) => handleDeleteClick(index, e)}
                disabled={files.length === 1}
              >
                🗑️
              </button>
            </div>
          </div>
        ))}
      </div>

      <div className="file-panel-footer">
        <details className="language-menu">
          <summary>Add file by type</summary>
          <div className="language-options">
            <button onClick={() => onAddFile('javascript')}>JavaScript</button>
            <button onClick={() => onAddFile('typescript')}>TypeScript</button>
            <button onClick={() => onAddFile('jsx')}>JSX</button>
            <button onClick={() => onAddFile('tsx')}>TSX</button>
            <button onClick={() => onAddFile('vue')}>Vue</button>
            <button onClick={() => onAddFile('html')}>HTML</button>
            <button onClick={() => onAddFile('css')}>CSS</button>
          </div>
        </details>
      </div>
    </div>
  )
}
