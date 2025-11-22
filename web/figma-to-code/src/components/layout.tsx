'use client'

import * as React from 'react'
import { Header } from '@/components/header'
import { Sidebar } from '@/components/sidebar'
import { DesignInput } from '@/components/design-input'
import { CodeEditorPane } from '@/components/code-editor-pane'
import { PreviewPane } from '@/components/preview-pane'
import { ActionBar } from '@/components/action-bar'

export function Layout() {
  const [sidebarOpen, setSidebarOpen] = React.useState(false)
  const [code, setCode] = React.useState('')

  const handlePaste = (text: string) => {
    // In a real app, this would parse the Figma JSON and generate code
    const generatedCode = `// Generated from Figma design
import React from 'react'

export default function Component() {
  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold">Generated Component</h1>
      <p className="text-gray-600">Based on your design input</p>
    </div>
  )
}`
    setCode(generatedCode)
  }

  return (
    <div className='flex h-screen flex-col overflow-hidden'>
      {/* Header */}
      <Header onMenuClick={() => setSidebarOpen(!sidebarOpen)} sidebarOpen={sidebarOpen} />

      {/* Main content area */}
      <div className='flex flex-1 overflow-hidden'>
        {/* Sidebar */}
        <Sidebar onClose={() => setSidebarOpen(false)} open={sidebarOpen} />

        {/* Content columns */}
        <div className='flex-1 flex flex-col lg:flex-row overflow-hidden'>
          {/* Design input (left column) */}
          <div className='w-full lg:w-1/3 overflow-auto order-3 lg:order-1'>
            <DesignInput onPaste={handlePaste} />
          </div>

          {/* Code editor (middle column) */}
          <div className='w-full lg:w-1/3 overflow-auto order-2 lg:order-2'>
            <CodeEditorPane code={code} />
          </div>

          {/* Preview (right column) */}
          <div className='w-full lg:w-1/3 overflow-auto order-1 lg:order-3'>
            <PreviewPane />
          </div>
        </div>
      </div>

      {/* Action bar (footer) */}
      <ActionBar />
    </div>
  )
}
