'use client'

import * as React from 'react'
import { Moon, Sun, Menu, X } from 'lucide-react'
import { useTheme } from '@/components/theme-provider'
import { Button } from '@/components/ui/button'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'

interface HeaderProps {
  onMenuClick?: () => void
  sidebarOpen?: boolean
}

export function Header({ onMenuClick, sidebarOpen = false }: HeaderProps) {
  const { theme, toggleTheme } = useTheme()

  return (
    <header className='sticky top-0 z-40 border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60'>
      <div className='flex h-16 items-center gap-md px-md lg:px-lg'>
        {/* Menu button for mobile */}
        <Button
          variant='ghost'
          size='icon'
          onClick={onMenuClick}
          className='lg:hidden'
          aria-label='Toggle sidebar'
          aria-expanded={sidebarOpen}
          aria-controls='sidebar'
        >
          {sidebarOpen ? (
            <X className='h-5 w-5' />
          ) : (
            <Menu className='h-5 w-5' />
          )}
        </Button>

        {/* Logo / Brand */}
        <div className='flex items-center gap-sm'>
          <div className='flex h-8 w-8 items-center justify-center rounded-md bg-primary text-primary-foreground font-bold'>
            FC
          </div>
          <h1 className='text-lg font-bold hidden sm:inline'>Figma to Code</h1>
        </div>

        {/* Spacer */}
        <div className='flex-1' />

        {/* Theme toggle */}
        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              variant='ghost'
              size='icon'
              onClick={toggleTheme}
              aria-label={`Switch to ${
                theme === 'light'
                  ? 'dark'
                  : theme === 'dark'
                    ? 'system'
                    : 'light'
              } theme`}
            >
              {theme === 'dark' || (theme === 'system' && true) ? (
                <Sun className='h-5 w-5 text-yellow-500' />
              ) : (
                <Moon className='h-5 w-5 text-slate-600' />
              )}
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            <p>Current: {theme}</p>
          </TooltipContent>
        </Tooltip>
      </div>
    </header>
  )
}
