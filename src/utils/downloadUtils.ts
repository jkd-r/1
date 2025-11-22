import JSZip from 'jszip'
import { FileContent } from '../types'

export async function downloadFilesAsZip(files: FileContent[], projectName: string = 'project'): Promise<void> {
  try {
    const zip = new JSZip()

    files.forEach((file) => {
      zip.file(file.name, file.content)
    })

    // Add package.json if not present
    if (!files.some((f) => f.name === 'package.json')) {
      const packageJson = {
        name: projectName.toLowerCase().replace(/\s+/g, '-'),
        version: '1.0.0',
        description: `Generated from Protocol EMR Editor`,
        type: 'module',
      }
      zip.file('package.json', JSON.stringify(packageJson, null, 2))
    }

    // Add README if not present
    if (!files.some((f) => f.name === 'README.md')) {
      zip.file('README.md', `# ${projectName}\n\nGenerated from Protocol EMR Code Editor`)
    }

    const blob = await zip.generateAsync({ type: 'blob' })
    downloadBlob(blob, `${projectName}-${Date.now()}.zip`)
  } catch (error) {
    throw new Error(`Failed to create zip file: ${error instanceof Error ? error.message : String(error)}`)
  }
}

export function downloadBlob(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

export function downloadFile(content: string, filename: string, mimeType: string = 'text/plain'): void {
  const blob = new Blob([content], { type: mimeType })
  downloadBlob(blob, filename)
}
