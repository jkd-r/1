'use client'

import * as React from 'react'
import { Settings } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'

interface SettingsDrawerProps {
  onOpenChange?: (open: boolean) => void
}

export function SettingsDrawer({ onOpenChange }: SettingsDrawerProps) {
  const [open, setOpen] = React.useState(false)

  const handleOpenChange = (newOpen: boolean) => {
    setOpen(newOpen)
    onOpenChange?.(newOpen)
  }

  return (
    <Sheet open={open} onOpenChange={handleOpenChange}>
      <Tooltip>
        <TooltipTrigger asChild>
          <SheetTrigger asChild>
            <Button
              variant='outline'
              size='icon'
              aria-label='Open settings'
              aria-expanded={open}
              aria-controls='settings-drawer'
            >
              <Settings className='h-5 w-5' />
            </Button>
          </SheetTrigger>
        </TooltipTrigger>
        <TooltipContent>
          <p>Settings</p>
        </TooltipContent>
      </Tooltip>

      <SheetContent>
        <SheetHeader>
          <SheetTitle>Settings & Configuration</SheetTitle>
          <SheetDescription>
            Customize your Figma to Code experience
          </SheetDescription>
        </SheetHeader>

        <div className='space-y-lg py-lg'>
          {/* Code Generation Settings */}
          <div>
            <h3 className='font-semibold text-sm mb-md'>Code Generation</h3>
            <div className='space-y-md text-sm'>
              <label className='flex items-center gap-md'>
                <input
                  type='checkbox'
                  defaultChecked
                  className='rounded border border-input'
                  aria-label='Optimize for performance'
                />
                <span>Optimize for performance</span>
              </label>
              <label className='flex items-center gap-md'>
                <input
                  type='checkbox'
                  defaultChecked
                  className='rounded border border-input'
                  aria-label='Include accessibility attributes'
                />
                <span>Include accessibility attributes</span>
              </label>
              <label className='flex items-center gap-md'>
                <input
                  type='checkbox'
                  defaultChecked
                  className='rounded border border-input'
                  aria-label='Generate TypeScript types'
                />
                <span>Generate TypeScript types</span>
              </label>
            </div>
          </div>

          {/* Output Format */}
          <div>
            <h3 className='font-semibold text-sm mb-md'>Output Format</h3>
            <div className='space-y-md text-sm'>
              <label className='flex items-center gap-md'>
                <input
                  type='radio'
                  name='format'
                  defaultChecked
                  className='rounded-full border border-input'
                  aria-label='Component format'
                />
                <span>Component (.tsx)</span>
              </label>
              <label className='flex items-center gap-md'>
                <input
                  type='radio'
                  name='format'
                  className='rounded-full border border-input'
                  aria-label='Inline HTML format'
                />
                <span>Inline HTML</span>
              </label>
            </div>
          </div>

          {/* Spacing */}
          <div>
            <h3 className='font-semibold text-sm mb-md'>Spacing</h3>
            <div className='space-y-md'>
              <label className='text-sm'>
                <span className='block mb-xs'>Base unit (px)</span>
                <input
                  type='number'
                  defaultValue={4}
                  min={1}
                  className='w-full px-sm py-xs border border-input rounded bg-background text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2'
                  aria-label='Base spacing unit'
                />
              </label>
            </div>
          </div>

          {/* Colors */}
          <div>
            <h3 className='font-semibold text-sm mb-md'>Colors</h3>
            <div className='space-y-md text-sm'>
              <label className='flex items-center gap-md'>
                <input
                  type='checkbox'
                  defaultChecked
                  className='rounded border border-input'
                  aria-label='Use CSS variables'
                />
                <span>Use CSS variables</span>
              </label>
              <label className='flex items-center gap-md'>
                <input
                  type='checkbox'
                  className='rounded border border-input'
                  aria-label='Include color palette'
                />
                <span>Include color palette</span>
              </label>
            </div>
          </div>

          {/* About */}
          <div className='border-t border-border pt-lg'>
            <h3 className='font-semibold text-sm mb-sm'>About</h3>
            <p className='text-xs text-muted-foreground'>
              Figma to Code v0.1.0
            </p>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  )
}
