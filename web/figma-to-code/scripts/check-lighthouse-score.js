import fs from 'fs'
import { fileURLToPath } from 'url'
import { dirname, join } from 'path'

const __filename = fileURLToPath(import.meta.url)
const __dirname = dirname(__filename)

// Read Lighthouse results
const resultsPath = join(__dirname, '../lighthouse-results.json')

try {
  const results = JSON.parse(fs.readFileSync(resultsPath, 'utf8'))
  
  // Extract performance scores
  const performance = results.lhr.categories.performance.score * 100
  const accessibility = results.lhr.categories.accessibility.score * 100
  const bestPractices = results.lhr.categories['best-practices'].score * 100
  const seo = results.lhr.categories.seo.score * 100
  
  console.log('\n📊 Lighthouse Performance Results')
  console.log('=====================================')
  console.log(`Performance: ${performance.toFixed(0)}/100 ${performance >= 90 ? '✅' : '❌'}`)
  console.log(`Accessibility: ${accessibility.toFixed(0)}/100 ${accessibility >= 90 ? '✅' : '❌'}`)
  console.log(`Best Practices: ${bestPractices.toFixed(0)}/100 ${bestPractices >= 90 ? '✅' : '❌'}`)
  console.log(`SEO: ${seo.toFixed(0)}/100 ${seo >= 90 ? '✅' : '❌'}`)
  
  // Check if all scores meet the >90 requirement
  const allScoresPass = performance >= 90 && accessibility >= 90 && bestPractices >= 90 && seo >= 90
  
  if (allScoresPass) {
    console.log('\n🎉 All Lighthouse scores meet the >90 requirement!')
    process.exit(0)
  } else {
    console.log('\n❌ Some Lighthouse scores are below the required 90 threshold.')
    console.log('Please review the detailed report for optimization opportunities.')
    process.exit(1)
  }
  
} catch (error) {
  console.error('Error reading Lighthouse results:', error.message)
  process.exit(1)
}