#!/usr/bin/env node

import { writeFileSync } from 'fs';
import $RefParser from '@apidevtools/json-schema-ref-parser';

async function dereferenceSchema() {
  const inputFile = process.argv[2];
  const outputFile = process.argv[3];
  
  if (!inputFile || !outputFile) {
    console.error('Usage: node dereference-schema.js <input> <output>');
    process.exit(1);
  }
  
  try {
    const schema = await $RefParser.dereference(inputFile);
    writeFileSync(outputFile, JSON.stringify(schema, null, 2));
    console.log('Schema dereferenced successfully');
  } catch (error) {
    console.error('Error dereferencing schema:', error.message);
    process.exit(1);
  }
}

dereferenceSchema();