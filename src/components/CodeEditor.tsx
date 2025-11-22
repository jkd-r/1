import React, { useCallback } from 'react'
import Editor from '@monaco-editor/react'
import { FileContent, LanguageMode } from '../types'
import './CodeEditor.css'

interface CodeEditorProps {
  file: FileContent
  onChange: (content: string) => void
  onLanguageChange: (language: LanguageMode) => void
}

const LANGUAGE_OPTIONS: { value: LanguageMode; label: string }[] = [
  { value: 'javascript', label: 'JavaScript' },
  { value: 'typescript', label: 'TypeScript' },
  { value: 'jsx', label: 'JSX' },
  { value: 'tsx', label: 'TSX' },
  { value: 'vue', label: 'Vue' },
  { value: 'html', label: 'HTML' },
  { value: 'css', label: 'CSS' },
]

export const CodeEditor: React.FC<CodeEditorProps> = ({ file, onChange, onLanguageChange }) => {
  const handleLanguageChange = useCallback(
    (e: React.ChangeEvent<HTMLSelectElement>) => {
      onLanguageChange(e.target.value as LanguageMode)
    },
    [onLanguageChange]
  )

  return (
    <div className="code-editor-container">
      <div className="editor-header">
        <div className="editor-title">
          <input
            type="text"
            value={file.name}
            readOnly
            className="file-name-input"
            title="File name"
          />
        </div>
        <select
          value={file.language}
          onChange={handleLanguageChange}
          className="language-select"
          title="Select programming language"
        >
          {LANGUAGE_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>
      <div className="editor-wrapper">
        <Editor
          height="100%"
          language={file.language}
          value={file.content}
          onChange={(value) => onChange(value || '')}
          theme="vs-dark"
          options={{
            minimap: { enabled: false },
            lineNumbers: 'on',
            scrollBeyondLastLine: false,
            automaticLayout: true,
            tabSize: 2,
            insertSpaces: true,
            formatOnPaste: true,
            wordWrap: 'on',
          }}
        />
      </div>
    </div>
  )
}
