'use client'

import * as React from 'react'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'
import { Button } from '@/components/ui/button'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'

interface SidebarProps {
  onClose?: () => void
  open?: boolean
}

const LANGUAGES = [
  { value: 'react', label: 'React' },
  { value: 'vue', label: 'Vue' },
  { value: 'svelte', label: 'Svelte' },
  { value: 'html', label: 'HTML' },
]

const FRAMEWORKS = [
  { value: 'tailwind', label: 'Tailwind' },
  { value: 'styled', label: 'Styled' },
  { value: 'css', label: 'CSS' },
]

export function Sidebar({ onClose, open = true }: SidebarProps) {
  const [language, setLanguage] = React.useState('react')
  const [framework, setFramework] = React.useState('tailwind')

  return (
    <>
      {/* Mobile overlay */}
      {open && (
        <div
          className='fixed inset-0 z-30 bg-black/50 lg:hidden'
          onClick={onClose}
          aria-hidden='true'
        />
      )}

      {/* Sidebar */}
      <aside
        id='sidebar'
        className={`fixed left-0 top-16 z-40 h-[calc(100vh-4rem)] w-64 transform border-r border-border bg-card transition-transform duration-300 ease-in-out lg:relative lg:top-0 lg:translate-x-0 ${
          open ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <div className='flex h-full flex-col gap-lg overflow-y-auto p-md'>
          {/* Language Selection */}
          <div>
            <h2 className='mb-sm text-sm font-semibold text-foreground'>
              Language
            </h2>
            <ToggleGroup
              type='single'
              value={language}
              onValueChange={setLanguage}
              className='flex flex-col gap-xs'
            >
              {LANGUAGES.map((lang) => (
                <ToggleGroupItem
                  key={lang.value}
                  value={lang.value}
                  className='w-full justify-start'
                  aria-label={`Select ${lang.label}`}
                >
                  <span className='text-left flex-1'>{lang.label}</span>
                </ToggleGroupItem>
              ))}
            </ToggleGroup>
          </div>

          {/* Framework Selection */}
          <div>
            <h2 className='mb-sm text-sm font-semibold text-foreground'>
              Styling
            </h2>
            <ToggleGroup
              type='single'
              value={framework}
              onValueChange={setFramework}
              className='flex flex-col gap-xs'
            >
              {FRAMEWORKS.map((fw) => (
                <ToggleGroupItem
                  key={fw.value}
                  value={fw.value}
                  className='w-full justify-start'
                  aria-label={`Select ${fw.label}`}
                >
                  <span className='text-left flex-1'>{fw.label}</span>
                </ToggleGroupItem>
              ))}
            </ToggleGroup>
          </div>

          {/* Quick presets */}
          <div className='border-t border-border pt-lg'>
            <h3 className='mb-sm text-xs font-semibold uppercase text-muted-foreground'>
              Presets
            </h3>
            <div className='flex flex-col gap-xs'>
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant='outline'
                    size='sm'
                    className='w-full justify-start'
                    onClick={() => {
                      setLanguage('react')
                      setFramework('tailwind')
                    }}
                  >
                    React + Tailwind
                  </Button>
                </TooltipTrigger>
                <TooltipContent side='right'>
                  <p>Quick setup for React with Tailwind CSS</p>
                </TooltipContent>
              </Tooltip>

              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant='outline'
                    size='sm'
                    className='w-full justify-start'
                    onClick={() => {
                      setLanguage('vue')
                      setFramework('tailwind')
                    }}
                  >
                    Vue + Tailwind
                  </Button>
                </TooltipTrigger>
                <TooltipContent side='right'>
                  <p>Quick setup for Vue with Tailwind CSS</p>
                </TooltipContent>
              </Tooltip>
            </div>
          </div>

          {/* Spacer */}
          <div className='flex-1' />

          {/* Current selection display */}
          <div className='border-t border-border pt-md text-xs text-muted-foreground'>
            <p>
              <strong>Language:</strong>{' '}
              {LANGUAGES.find((l) => l.value === language)?.label}
            </p>
            <p>
              <strong>Styling:</strong>{' '}
              {FRAMEWORKS.find((f) => f.value === framework)?.label}
            </p>
          </div>
        </div>
      </aside>
    </>
  )
}
