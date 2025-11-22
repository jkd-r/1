'use client'

import * as React from 'react'
import { RotateCcw, Maximize2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'

interface PreviewPaneProps {
  htmlContent?: string
}

export function PreviewPane({
  htmlContent = '<div class="flex items-center justify-center h-screen"><p class="text-lg">Preview will appear here</p></div>',
}: PreviewPaneProps) {
  const iframeRef = React.useRef<HTMLIFrameElement>(null)
  const [isFullscreen, setIsFullscreen] = React.useState(false)

  const handleRefresh = () => {
    if (iframeRef.current) {
      iframeRef.current.src = iframeRef.current.src
    }
  }

  const handleFullscreen = () => {
    if (iframeRef.current && iframeRef.current.requestFullscreen) {
      iframeRef.current.requestFullscreen()
    }
  }

  React.useEffect(() => {
    if (iframeRef.current) {
      const doc = iframeRef.current.contentDocument
      if (doc) {
        doc.open()
        doc.write(`
          <!DOCTYPE html>
          <html lang="en">
          <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <script src="https://cdn.tailwindcss.com"></script>
            <style>
              body { margin: 0; padding: 0; }
            </style>
          </head>
          <body>
            ${htmlContent}
          </body>
          </html>
        `)
        doc.close()
      }
    }
  }, [htmlContent])

  return (
    <div className='h-full flex flex-col bg-background border-b lg:border-b-0 lg:border-l border-border'>
      {/* Header */}
      <div className='flex items-center justify-between border-b border-border p-md lg:p-lg gap-md'>
        <h2 className='text-sm font-semibold text-foreground'>Preview</h2>
        <div className='flex gap-xs'>
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant='ghost'
                size='sm'
                onClick={handleRefresh}
                aria-label='Refresh preview'
              >
                <RotateCcw className='h-4 w-4' />
              </Button>
            </TooltipTrigger>
            <TooltipContent>
              <p>Refresh preview</p>
            </TooltipContent>
          </Tooltip>

          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant='ghost'
                size='sm'
                onClick={handleFullscreen}
                aria-label='Open preview fullscreen'
              >
                <Maximize2 className='h-4 w-4' />
              </Button>
            </TooltipTrigger>
            <TooltipContent>
              <p>Fullscreen</p>
            </TooltipContent>
          </Tooltip>
        </div>
      </div>

      {/* Preview content */}
      <div className='flex-1 overflow-hidden bg-white dark:bg-slate-950'>
        <iframe
          ref={iframeRef}
          title='Preview'
          className='w-full h-full border-0'
          sandbox='allow-scripts allow-same-origin'
          style={{ backgroundColor: 'white' }}
          aria-label='Preview of generated component'
        />
      </div>
    </div>
  )
}
