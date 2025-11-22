import { LanguageMode } from '../types'

export const EDITOR_OPTIONS = {
  minimap: { enabled: false },
  lineNumbers: 'on' as const,
  scrollBeyondLastLine: false,
  automaticLayout: true,
  tabSize: 2,
  insertSpaces: true,
  formatOnPaste: true,
  wordWrap: 'on' as const,
  fontSize: 14,
  fontFamily: "'Fira Code', 'Monaco', 'Menlo', monospace",
  renderWhitespace: 'none' as const,
  suggestOnTriggerCharacters: true,
  acceptSuggestionOnCommitCharacter: true,
  acceptSuggestionOnEnter: 'on' as const,
}

export const LANGUAGE_DEFAULTS: Record<LanguageMode, { icon: string; color: string }> = {
  javascript: { icon: '🟨', color: '#f7df1e' },
  typescript: { icon: '🔵', color: '#3178c6' },
  jsx: { icon: '⚛️', color: '#61dafb' },
  tsx: { icon: '⚛️', color: '#3178c6' },
  vue: { icon: '💚', color: '#4fc08d' },
  html: { icon: '📄', color: '#e34c26' },
  css: { icon: '🎨', color: '#563d7c' },
}

export const PREVIEW_UPDATE_DELAY = 300 // milliseconds

export const MAX_HISTORY_ENTRIES = 50

export const TOAST_DURATION = 3000 // milliseconds

export const LANGUAGE_FILE_EXTENSIONS: Record<LanguageMode, string> = {
  javascript: 'js',
  typescript: 'ts',
  jsx: 'jsx',
  tsx: 'tsx',
  vue: 'vue',
  html: 'html',
  css: 'css',
}
