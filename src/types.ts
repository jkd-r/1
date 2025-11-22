export type LanguageMode = 'javascript' | 'typescript' | 'jsx' | 'tsx' | 'vue' | 'html' | 'css'

export interface FileContent {
  name: string
  language: LanguageMode
  content: string
}

export interface HistoryEntry {
  id: string
  timestamp: number
  files: FileContent[]
  designInputs: DesignInputs
  previewOutput?: string
}

export interface DesignInputs {
  componentName: string
  description: string
}

export interface ToastMessage {
  id: string
  message: string
  type: 'success' | 'error' | 'info'
  duration?: number
}
