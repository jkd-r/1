import React, { useCallback } from 'react'
import { FileContent, DesignInputs } from '../types'
import { downloadFilesAsZip } from '../utils/downloadUtils'
import './Toolbar.css'

interface ToolbarProps {
  files: FileContent[]
  designInputs: DesignInputs
  onSaveToHistory: () => void
  onShowToast: (message: string, type: 'success' | 'error') => void
}

export const Toolbar: React.FC<ToolbarProps> = ({
  files,
  designInputs,
  onSaveToHistory,
  onShowToast,
}) => {
  const handleDownload = useCallback(async () => {
    try {
      const projectName = designInputs.componentName || 'project'
      await downloadFilesAsZip(files, projectName)
      onShowToast(`Downloaded "${projectName}" as zip`, 'success')
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : String(err)
      onShowToast(`Download failed: ${errorMessage}`, 'error')
    }
  }, [files, designInputs, onShowToast])

  const handleSaveHistory = useCallback(() => {
    onSaveToHistory()
    onShowToast('Saved to history', 'success')
  }, [onSaveToHistory, onShowToast])

  return (
    <div className="toolbar">
      <div className="toolbar-section">
        <label htmlFor="component-name">Component:</label>
        <input
          id="component-name"
          type="text"
          value={designInputs.componentName}
          readOnly
          className="design-input"
          placeholder="Component name"
        />
      </div>

      <div className="toolbar-section">
        <button
          className="toolbar-btn save-btn"
          onClick={handleSaveHistory}
          title="Save current state to history"
        >
          💾 Save to History
        </button>
        <button
          className="toolbar-btn download-btn"
          onClick={handleDownload}
          title="Download all files as ZIP"
        >
          ⬇️ Download ZIP
        </button>
      </div>
    </div>
  )
}
