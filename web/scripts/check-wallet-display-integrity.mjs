import { readdirSync, readFileSync } from 'node:fs'
import { join } from 'node:path'

const tradingRoot = join(import.meta.dirname, '..', 'src', 'features', 'trading')

const forbiddenPatterns = [
  { label: 'totalBalance - reservedBalance', regex: /totalBalance\s*-\s*reservedBalance/ },
  { label: 'total - reserved (subtraction)', regex: /\btotal\b\s*-\s*\breserved\b/i },
]

function collectSourceFiles(directory) {
  const entries = readdirSync(directory, { withFileTypes: true })
  const files = []

  for (const entry of entries) {
    const path = join(directory, entry.name)
    if (entry.isDirectory()) {
      files.push(...collectSourceFiles(path))
      continue
    }
    if (entry.name.endsWith('.ts') || entry.name.endsWith('.tsx')) {
      files.push(path)
    }
  }

  return files
}

const violations = []

for (const filePath of collectSourceFiles(tradingRoot)) {
  const content = readFileSync(filePath, 'utf8')
  const relativePath = filePath.replace(/\\/g, '/').split('/web/')[1] ?? filePath

  for (const { label, regex } of forbiddenPatterns) {
    if (regex.test(content)) {
      violations.push(`${relativePath}: forbidden pattern "${label}"`)
    }
  }
}

if (violations.length > 0) {
  console.error('Wallet display integrity check failed:\n' + violations.join('\n'))
  process.exit(1)
}

console.log('Wallet display integrity check passed.')
