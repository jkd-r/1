import React, { useEffect, useRef, useState } from 'react'
import { FileContent } from '../types'
import { compileCode } from '../utils/codeCompiler'
import './LivePreview.css'

interface LivePreviewProps {
  files: FileContent[]
  isLoading: boolean
  error: string | null
}

export const LivePreview: React.FC<LivePreviewProps> = ({ files, isLoading, error }) => {
  const iframeRef = useRef<HTMLIFrameElement>(null)
  const [previewError, setPreviewError] = useState<string | null>(null)

  useEffect(() => {
    const updatePreview = async () => {
      if (!iframeRef.current || files.length === 0) {
        return
      }

      try {
        setPreviewError(null)

        // Use the first file as the main entry point
        const mainFile = files[0]
        const html = await compileCode(mainFile.content, mainFile.language)

        const iframe = iframeRef.current
        iframe.srcdoc = html

        // Handle runtime errors in the iframe
        iframe.onload = () => {
          try {
            const iframeDoc = iframe.contentDocument
            if (!iframeDoc) {
              setPreviewError('Could not access iframe')
              return
            }

            // Capture console errors
            const iframeWindow = iframe.contentWindow
            if (iframeWindow) {
              iframeWindow.onerror = (message, source, lineno, colno, error) => {
                setPreviewError(`${message} (line ${lineno})`)
                return true
              }
            }
          } catch (err) {
            console.error('Error setting up iframe error handling:', err)
          }
        }
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : String(err)
        setPreviewError(errorMessage)
      }
    }

    updatePreview()
  }, [files])

  return (
    <div className="live-preview-container">
      <div className="preview-header">
        <h3>Live Preview</h3>
        {isLoading && <span className="loading-indicator">Compiling...</span>}
      </div>
      {error && (
        <div className="preview-error">
          <strong>Editor Error:</strong> {error}
        </div>
      )}
      {previewError && (
        <div className="preview-error">
          <strong>Preview Error:</strong> {previewError}
        </div>
      )}
      <iframe ref={iframeRef} className="preview-iframe" title="Live Preview" sandbox="allow-scripts" />
    </div>
  )
}
