'use client'

import * as React from 'react'
import { Copy, Download } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'

interface CodeEditorPaneProps {
  code?: string
  language?: string
}

const SAMPLE_CODE = `import React from 'react'

export default function Component() {
  return (
    <div className="flex items-center justify-center h-screen">
      <h1 className="text-3xl font-bold">
        Your generated code will appear here
      </h1>
    </div>
  )
}`

export function CodeEditorPane({
  code = SAMPLE_CODE,
  language = 'tsx',
}: CodeEditorPaneProps) {
  const [activeTab, setActiveTab] = React.useState('code')
  const [copied, setCopied] = React.useState(false)

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(code)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch (err) {
      console.error('Failed to copy:', err)
    }
  }

  const handleDownload = () => {
    const element = document.createElement('a')
    const file = new Blob([code], { type: 'text/plain' })
    element.href = URL.createObjectURL(file)
    element.download = `component.${language === 'tsx' ? 'tsx' : 'jsx'}`
    document.body.appendChild(element)
    element.click()
    document.body.removeChild(element)
  }

  return (
    <div className='h-full flex flex-col bg-card border-r border-border'>
      {/* Header */}
      <div className='flex items-center justify-between border-b border-border p-md lg:p-lg gap-md'>
        <h2 className='text-sm font-semibold text-foreground'>Code</h2>
        <div className='flex gap-xs'>
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant='ghost'
                size='sm'
                onClick={handleCopy}
                aria-label='Copy code to clipboard'
              >
                <Copy className='h-4 w-4' />
              </Button>
            </TooltipTrigger>
            <TooltipContent>
              <p>{copied ? 'Copied!' : 'Copy code'}</p>
            </TooltipContent>
          </Tooltip>

          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant='ghost'
                size='sm'
                onClick={handleDownload}
                aria-label='Download code'
              >
                <Download className='h-4 w-4' />
              </Button>
            </TooltipTrigger>
            <TooltipContent>
              <p>Download code</p>
            </TooltipContent>
          </Tooltip>
        </div>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab} className='flex-1'>
        <div className='border-b border-border px-md lg:px-lg'>
          <TabsList className='h-auto rounded-none border-0 bg-transparent p-0'>
            <TabsTrigger
              value='code'
              className='rounded-none border-b-2 border-transparent data-[state=active]:border-primary px-md py-sm'
            >
              Code
            </TabsTrigger>
            <TabsTrigger
              value='dependencies'
              className='rounded-none border-b-2 border-transparent data-[state=active]:border-primary px-md py-sm'
            >
              Dependencies
            </TabsTrigger>
          </TabsList>
        </div>

        {/* Code Content */}
        <TabsContent value='code' className='flex-1 m-0'>
          <div className='h-full overflow-auto'>
            <pre className='p-md lg:p-lg font-mono text-xs text-muted-foreground'>
              <code>{code}</code>
            </pre>
          </div>
        </TabsContent>

        {/* Dependencies Content */}
        <TabsContent value='dependencies' className='flex-1 m-0'>
          <div className='p-md lg:p-lg'>
            <div className='space-y-md'>
              <div>
                <h3 className='text-sm font-semibold text-foreground mb-sm'>
                  Required Packages
                </h3>
                <ul className='space-y-xs text-sm text-muted-foreground'>
                  <li>
                    <code className='bg-muted px-xs py-0.5 rounded'>
                      react@^18.2.0
                    </code>
                  </li>
                  <li>
                    <code className='bg-muted px-xs py-0.5 rounded'>
                      tailwindcss@^3.3.0
                    </code>
                  </li>
                </ul>
              </div>
              <div>
                <h3 className='text-sm font-semibold text-foreground mb-sm'>
                  Installation
                </h3>
                <code className='block bg-muted p-sm rounded text-xs'>
                  npm install react tailwindcss
                </code>
              </div>
            </div>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  )
}
