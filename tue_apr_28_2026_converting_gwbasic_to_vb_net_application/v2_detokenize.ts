import * as fs from "fs";
import { decoder } from "./converter/decoder";

function usage() {
  console.log("Usage:");
  console.log('  npx ts-node --transpile-only detokenize.ts <input.bas> <output.bas>');
  process.exit(2);
}

const args = process.argv.slice(2);
if (args.length < 2) usage();

const [inputPath, outputPath] = args;

const buf = fs.readFileSync(inputPath);
const bytes: number[] = Array.from(buf); // number[] 0-255

const d = new decoder();
d.setStream(bytes);

const text = d.decode();

fs.writeFileSync(outputPath, text, "utf8");
console.log(`Wrote ${outputPath}`);