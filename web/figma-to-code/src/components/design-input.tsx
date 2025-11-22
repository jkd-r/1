'use client'

import * as React from 'react'
import { Upload, Clipboard } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'

interface DesignInputProps {
  onFileUpload?: (file: File) => void
  onPaste?: (text: string) => void
}

export function DesignInput({ onFileUpload, onPaste }: DesignInputProps) {
  const [isDragging, setIsDragging] = React.useState(false)
  const [pastedContent, setPastedContent] = React.useState('')
  const fileInputRef = React.useRef<HTMLInputElement>(null)

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragging(true)
  }

  const handleDragLeave = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragging(false)
  }

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragging(false)

    const files = e.dataTransfer.files
    if (files.length > 0) {
      const file = files[0]
      if (file.type === 'application/json' || file.name.endsWith('.json')) {
        onFileUpload?.(file)
      }
    }
  }

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.currentTarget.files
    if (files && files.length > 0) {
      onFileUpload?.(files[0])
    }
  }

  const handlePaste = (e: React.ClipboardEvent<HTMLTextAreaElement>) => {
    const text = e.clipboardData.getData('text')
    setPastedContent(text)
    onPaste?.(text)
  }

  return (
    <div className='h-full flex flex-col gap-md p-md lg:p-lg border-r border-border'>
      <div>
        <h2 className='text-sm font-semibold text-foreground mb-sm'>
          Upload Design
        </h2>

        {/* Drag and drop area */}
        <div
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
          className={`flex flex-col items-center justify-center gap-md rounded-lg border-2 border-dashed transition-colors p-lg cursor-pointer ${
            isDragging
              ? 'border-primary bg-primary/5'
              : 'border-border hover:border-primary/50 hover:bg-muted/50'
          }`}
          onClick={() => fileInputRef.current?.click()}
          role='button'
          tabIndex={0}
          aria-label='Drag and drop area for design files'
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              fileInputRef.current?.click()
            }
          }}
        >
          <Upload className='h-8 w-8 text-muted-foreground' />
          <div className='text-center'>
            <p className='text-sm font-medium text-foreground'>
              Drop your design here
            </p>
            <p className='text-xs text-muted-foreground'>
              or click to browse
            </p>
          </div>
          <input
            ref={fileInputRef}
            type='file'
            accept='.json'
            onChange={handleFileSelect}
            className='hidden'
            aria-label='File input'
          />
        </div>
      </div>

      {/* Paste input */}
      <div>
        <h3 className='text-sm font-semibold text-foreground mb-sm'>
          Or paste JSON
        </h3>
        <textarea
          placeholder='Paste your design JSON here...'
          onPaste={handlePaste}
          onChange={(e) => setPastedContent(e.target.value)}
          value={pastedContent}
          className='w-full h-40 p-sm rounded-md border border-input bg-background text-foreground placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 resize-none'
          aria-label='Paste design JSON'
        />
      </div>

      {/* Action buttons */}
      <div className='flex gap-xs'>
        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              variant='outline'
              size='sm'
              onClick={() => fileInputRef.current?.click()}
              className='flex-1'
            >
              <Upload className='h-4 w-4 mr-xs' />
              Browse
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            <p>Choose a design file from your computer</p>
          </TooltipContent>
        </Tooltip>

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              variant='outline'
              size='sm'
              onClick={async () => {
                try {
                  const text = await navigator.clipboard.readText()
                  setPastedContent(text)
                  onPaste?.(text)
                } catch (err) {
                  console.error('Failed to read clipboard:', err)
                }
              }}
              className='flex-1'
            >
              <Clipboard className='h-4 w-4 mr-xs' />
              Paste
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            <p>Paste from clipboard</p>
          </TooltipContent>
        </Tooltip>
      </div>

      {/* Content status */}
      {pastedContent && (
        <div className='rounded-md border border-green-200 bg-green-50 p-sm dark:border-green-900 dark:bg-green-900/20'>
          <p className='text-xs text-green-800 dark:text-green-200'>
            ✓ Content loaded
          </p>
        </div>
      )}
    </div>
  )
}
