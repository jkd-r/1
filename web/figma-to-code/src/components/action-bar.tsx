'use client'

import * as React from 'react'
import { Download, Share2, Zap } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'
import { SettingsDrawer } from '@/components/settings-drawer'

interface ActionBarProps {
  onExport?: () => void
  onShare?: () => void
}

export function ActionBar({ onExport, onShare }: ActionBarProps) {
  const [exporting, setExporting] = React.useState(false)

  const handleExport = async () => {
    setExporting(true)
    try {
      await onExport?.()
    } finally {
      setExporting(false)
    }
  }

  const handleShare = async () => {
    if (navigator.share) {
      try {
        await navigator.share({
          title: 'Figma to Code',
          text: 'Check out this generated component',
          url: window.location.href,
        })
      } catch (err) {
        if (err instanceof Error && err.name !== 'AbortError') {
          console.error('Share failed:', err)
        }
      }
    } else {
      // Fallback: copy to clipboard
      try {
        await navigator.clipboard.writeText(window.location.href)
        alert('Link copied to clipboard!')
      } catch (err) {
        console.error('Failed to copy link:', err)
      }
    }
    onShare?.()
  }

  return (
    <div className='border-t border-border bg-card p-md lg:p-lg flex items-center gap-md justify-between'>
      {/* Left section - Info */}
      <div className='hidden sm:flex items-center gap-sm text-xs text-muted-foreground'>
        <Zap className='h-4 w-4' />
        <span>Ready to export</span>
      </div>

      {/* Right section - Actions */}
      <div className='flex items-center gap-xs ml-auto'>
        <SettingsDrawer />

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              variant='outline'
              size='sm'
              onClick={handleShare}
              aria-label='Share'
            >
              <Share2 className='h-4 w-4 mr-xs' />
              <span className='hidden sm:inline'>Share</span>
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            <p>Share this project</p>
          </TooltipContent>
        </Tooltip>

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              size='sm'
              onClick={handleExport}
              disabled={exporting}
              aria-label='Export code'
            >
              <Download className='h-4 w-4 mr-xs' />
              <span className='hidden sm:inline'>
                {exporting ? 'Exporting...' : 'Export'}
              </span>
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            <p>Download all code and assets</p>
          </TooltipContent>
        </Tooltip>
      </div>
    </div>
  )
}
