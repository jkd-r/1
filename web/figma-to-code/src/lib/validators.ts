/**
 * Code validators for different languages and frameworks
 * Used to validate generated code before display
 */

export interface ValidationResult {
  isValid: boolean;
  error?: string;
  details?: Record<string, unknown>;
}

/**
 * Validates JavaScript code by attempting to create a function from it
 */
export function validateJavaScript(code: string): ValidationResult {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Code is empty' };
  }

  try {
    // eslint-disable-next-line no-new-func
    new Function(code);
    return { isValid: true };
  } catch (error) {
    return {
      isValid: false,
      error: error instanceof Error ? error.message : 'Invalid JavaScript syntax',
    };
  }
}

/**
 * Validates TypeScript code
 * Checks for common TypeScript patterns and syntax
 */
export function validateTypeScript(code: string): ValidationResult {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Code is empty' };
  }

  // Basic TypeScript validation using pattern matching
  const lines = code.split('\n');
  const issues: string[] = [];

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i].trim();
    const lineNum = i + 1;

    // Check for unclosed braces
    const openBraces = (line.match(/{/g) || []).length;
    const closeBraces = (line.match(/}/g) || []).length;

    if (openBraces !== closeBraces) {
      // This is just a line check, not comprehensive
      continue;
    }

    // Check for interface definitions without body
    if (line.startsWith('interface ') && !line.includes('{')) {
      issues.push(`Line ${lineNum}: Interface definition missing opening brace`);
    }

    // Check for function declarations without body
    if (line.startsWith('function ') && !line.includes('{')) {
      issues.push(`Line ${lineNum}: Function declaration missing opening brace`);
    }

    // Check for incomplete imports
    if (line.startsWith('import ') && !line.includes('from')) {
      issues.push(`Line ${lineNum}: Import statement incomplete`);
    }
  }

  if (issues.length > 0) {
    return {
      isValid: false,
      error: issues.join('; '),
    };
  }

  // Try to validate as JavaScript since TypeScript compiles to JS
  try {
    // Remove type annotations for basic validation
    const stripped = code
      .replace(/:\s*[A-Za-z<>[\]|&\s,]+\s*(?=[=\);])/g, '') // Remove type annotations
      .replace(/as\s+[A-Za-z<>[\]\s]*\b/g, ''); // Remove type assertions

    // eslint-disable-next-line no-new-func
    new Function(stripped);
    return { isValid: true };
  } catch (error) {
    return {
      isValid: false,
      error: error instanceof Error ? error.message : 'Invalid TypeScript syntax',
    };
  }
}

/**
 * Validates HTML code
 */
export function validateHTML(code: string): ValidationResult {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Code is empty' };
  }

  if (typeof window === 'undefined') {
    // Server-side validation (basic)
    if (!code.includes('<!DOCTYPE html>') && !code.includes('<html')) {
      return { isValid: false, error: 'Missing HTML structure' };
    }
    return { isValid: true };
  }

  try {
    const parser = new DOMParser();
    const doc = parser.parseFromString(code, 'text/html');
    const errors = doc.getElementsByTagName('parsererror');

    if (errors.length > 0) {
      const errorMessage = errors[0].textContent || 'Invalid HTML';
      return {
        isValid: false,
        error: `HTML Parse Error: ${errorMessage}`,
      };
    }

    return { isValid: true };
  } catch (error) {
    return {
      isValid: false,
      error: error instanceof Error ? error.message : 'Invalid HTML',
    };
  }
}

/**
 * Validates CSS code
 */
export function validateCSS(code: string): ValidationResult {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Code is empty' };
  }

  if (typeof document === 'undefined') {
    // Server-side validation (basic)
    try {
      // Basic CSS syntax checks
      const rules = code.match(/{[^}]*}/g) || [];
      if (rules.length === 0 && code.includes('{')) {
        return { isValid: false, error: 'No valid CSS rules found' };
      }
      return { isValid: true };
    } catch {
      return { isValid: false, error: 'Invalid CSS syntax' };
    }
  }

  try {
    // Create and inject a style element to validate CSS
    const style = document.createElement('style');
    style.textContent = code;
    document.head.appendChild(style);

    // Check if there are any parsing errors
    const sheet = style.sheet;
    if (!sheet) {
      document.head.removeChild(style);
      return { isValid: false, error: 'CSS failed to parse' };
    }

    document.head.removeChild(style);
    return { isValid: true };
  } catch (error) {
    return {
      isValid: false,
      error: error instanceof Error ? error.message : 'Invalid CSS',
    };
  }
}

/**
 * Validates React/JSX code
 */
export function validateReact(code: string): ValidationResult {
  // React code is valid JavaScript, so we validate as JS
  return validateJavaScript(code);
}

/**
 * Validates Vue single file component code
 */
export function validateVue(code: string): ValidationResult {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Code is empty' };
  }

  // Basic Vue SFC validation
  const hasTemplate = code.includes('<template');
  const hasScript = code.includes('<script');

  if (!hasTemplate && !hasScript) {
    return { isValid: false, error: 'Missing template or script section' };
  }

  // Check for matching tags
  const templateCount = (code.match(/<template/g) || []).length;
  const templateCloseCount = (code.match(/<\/template>/g) || []).length;

  if (templateCount !== templateCloseCount) {
    return { isValid: false, error: 'Mismatched template tags' };
  }

  const scriptCount = (code.match(/<script/g) || []).length;
  const scriptCloseCount = (code.match(/<\/script>/g) || []).length;

  if (scriptCount !== scriptCloseCount) {
    return { isValid: false, error: 'Mismatched script tags' };
  }

  return { isValid: true };
}

/**
 * Generic code validator that routes to language-specific validators
 */
export function validateCode(language: string, code: string): ValidationResult {
  switch (language.toLowerCase()) {
    case 'javascript':
    case 'js':
    case 'jsx':
      return validateJavaScript(code);

    case 'typescript':
    case 'ts':
    case 'tsx':
      return validateTypeScript(code);

    case 'html':
      return validateHTML(code);

    case 'css':
      return validateCSS(code);

    case 'react':
    case 'jsx':
      return validateReact(code);

    case 'vue':
      return validateVue(code);

    default:
      return { isValid: true }; // Unknown language - pass through
  }
}
