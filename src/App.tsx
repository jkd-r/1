import React, { useState, useCallback, useMemo, useRef } from 'react'
import { FileContent, HistoryEntry, LanguageMode, DesignInputs, ToastMessage } from './types'
import { useLocalStorage } from './hooks/useLocalStorage'
import { useDebounce } from './hooks/useDebounce'
import { CodeEditor } from './components/CodeEditor'
import { LivePreview } from './components/LivePreview'
import { FilePanel } from './components/FilePanel'
import { HistoryPanel } from './components/HistoryPanel'
import { Toolbar } from './components/Toolbar'
import { ToastContainer } from './components/ToastContainer'
import './App.css'

const DEFAULT_FILES: FileContent[] = [
  {
    name: 'index.jsx',
    language: 'jsx',
    content: `export default function App() {
  return (
    <div style={{ padding: '20px', fontFamily: 'system-ui' }}>
      <h1>🎉 Welcome to Protocol EMR Editor</h1>
      <p>Edit this code and see the preview update live!</p>
      <button style={{ 
        padding: '10px 20px',
        backgroundColor: '#0e639c',
        color: 'white',
        border: 'none',
        borderRadius: '4px',
        cursor: 'pointer'
      }}>
        Click me
      </button>
    </div>
  )
}

const root = ReactDOM.createRoot(document.getElementById('root'))
root.render(<App />)`,
  },
]

const DEFAULT_DESIGN_INPUTS: DesignInputs = {
  componentName: 'My Component',
  description: 'A sample component',
}

function generateHistoryId(): string {
  return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
}

export default function App() {
  // Files management
  const [files, setFiles] = useLocalStorage<FileContent[]>('emr-files', DEFAULT_FILES)
  const [selectedFileIndex, setSelectedFileIndex] = useState(0)

  // Design inputs
  const [designInputs, setDesignInputs] = useLocalStorage<DesignInputs>(
    'emr-design-inputs',
    DEFAULT_DESIGN_INPUTS
  )

  // History
  const [history, setHistory] = useLocalStorage<HistoryEntry[]>('emr-history', [])

  // Preview state
  const [compilationError, setCompilationError] = useState<string | null>(null)
  const [isCompiling, setIsCompiling] = useState(false)

  // Toasts
  const [toasts, setToasts] = useState<ToastMessage[]>([])
  const toastIdRef = useRef(0)

  // Debounce file changes to avoid too frequent updates
  const debouncedFiles = useDebounce(files, 300)

  // Add toast notification
  const showToast = useCallback((message: string, type: 'success' | 'error' | 'info' = 'info') => {
    const id = `toast-${toastIdRef.current++}`
    const newToast: ToastMessage = {
      id,
      message,
      type,
      duration: 3000,
    }
    setToasts((prev) => [...prev, newToast])
  }, [])

  // Remove toast
  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id))
  }, [])

  // File operations
  const handleFileChange = useCallback((content: string) => {
    setFiles((prev) => {
      const newFiles = [...prev]
      newFiles[selectedFileIndex] = {
        ...newFiles[selectedFileIndex],
        content,
      }
      return newFiles
    })
    setCompilationError(null)
  }, [selectedFileIndex, setFiles])

  const handleLanguageChange = useCallback((language: LanguageMode) => {
    setFiles((prev) => {
      const newFiles = [...prev]
      newFiles[selectedFileIndex] = {
        ...newFiles[selectedFileIndex],
        language,
      }
      return newFiles
    })
  }, [selectedFileIndex, setFiles])

  const handleAddFile = useCallback((language: LanguageMode) => {
    const fileCount = files.length
    const newFileName = `file${fileCount}.${language === 'jsx' ? 'jsx' : language === 'tsx' ? 'tsx' : language === 'vue' ? 'vue' : 'js'}`

    const newFile: FileContent = {
      name: newFileName,
      language,
      content: getDefaultFileContent(language),
    }

    setFiles((prev) => [...prev, newFile])
    setSelectedFileIndex(files.length)
  }, [files, setFiles])

  const handleDeleteFile = useCallback(
    (index: number) => {
      if (files.length <= 1) {
        showToast('Cannot delete the last file', 'error')
        return
      }

      setFiles((prev) => prev.filter((_, i) => i !== index))

      if (selectedFileIndex >= files.length - 1) {
        setSelectedFileIndex(Math.max(0, files.length - 2))
      }
    },
    [files, selectedFileIndex, setFiles, showToast]
  )

  const handleCopyFile = useCallback((index: number) => {
    // Feedback already shown in FilePanel
  }, [])

  // History operations
  const handleSaveToHistory = useCallback(() => {
    const entry: HistoryEntry = {
      id: generateHistoryId(),
      timestamp: Date.now(),
      files: JSON.parse(JSON.stringify(files)),
      designInputs: JSON.parse(JSON.stringify(designInputs)),
    }

    setHistory((prev) => {
      const updated = [entry, ...prev]
      // Keep only last 50 entries
      return updated.slice(0, 50)
    })
  }, [files, designInputs, setHistory])

  const handleSelectHistoryEntry = useCallback(
    (entry: HistoryEntry) => {
      setFiles(JSON.parse(JSON.stringify(entry.files)))
      setDesignInputs(JSON.parse(JSON.stringify(entry.designInputs)))
      setSelectedFileIndex(0)
      showToast(`Restored "${entry.designInputs.componentName}" from history`, 'success')
    },
    [setFiles, setDesignInputs, showToast]
  )

  const handleClearHistory = useCallback(() => {
    setHistory([])
    showToast('History cleared', 'success')
  }, [setHistory, showToast])

  const currentFile = files[selectedFileIndex] || files[0]

  return (
    <div className="app">
      <Toolbar
        files={files}
        designInputs={designInputs}
        onSaveToHistory={handleSaveToHistory}
        onShowToast={showToast}
      />

      <div className="editor-layout">
        <FilePanel
          files={files}
          selectedFileIndex={selectedFileIndex}
          onSelectFile={setSelectedFileIndex}
          onAddFile={handleAddFile}
          onDeleteFile={handleDeleteFile}
          onCopyFile={handleCopyFile}
          onShowToast={showToast}
        />

        <div className="editor-area">
          {currentFile && (
            <CodeEditor
              file={currentFile}
              onChange={handleFileChange}
              onLanguageChange={handleLanguageChange}
            />
          )}
        </div>

        <LivePreview
          files={debouncedFiles}
          isLoading={isCompiling}
          error={compilationError}
        />
      </div>

      <HistoryPanel
        history={history}
        onSelectEntry={handleSelectHistoryEntry}
        onClearHistory={handleClearHistory}
      />

      <ToastContainer messages={toasts} onClose={removeToast} />
    </div>
  )
}

function getDefaultFileContent(language: LanguageMode): string {
  switch (language) {
    case 'jsx':
      return `export default function Component() {
  return (
    <div>Hello from JSX</div>
  )
}`
    case 'tsx':
      return `import React from 'react'

interface Props {
  name?: string
}

export default function Component({ name = 'World' }: Props) {
  return (
    <div>Hello, {name}!</div>
  )
}`
    case 'vue':
      return `<template>
  <div>
    <h1>{{ message }}</h1>
    <button @click="increment">Count: {{ count }}</button>
  </div>
</template>

<script>
export default {
  data() {
    return {
      message: 'Hello from Vue!',
      count: 0
    }
  },
  methods: {
    increment() {
      this.count++
    }
  }
}
</script>`
    case 'html':
      return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Page</title>
</head>
<body>
  <h1>Welcome</h1>
  <p>Your HTML content here</p>
</body>
</html>`
    case 'css':
      return `body {
  font-family: system-ui, sans-serif;
  background-color: #f5f5f5;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}`
    case 'typescript':
      return `interface User {
  id: number
  name: string
  email: string
}

function createUser(name: string, email: string): User {
  return {
    id: Date.now(),
    name,
    email
  }
}`
    case 'javascript':
    default:
      return `function hello(name = 'World') {
  return \`Hello, \${name}!\`
}

console.log(hello('Developer'))`
  }
}
