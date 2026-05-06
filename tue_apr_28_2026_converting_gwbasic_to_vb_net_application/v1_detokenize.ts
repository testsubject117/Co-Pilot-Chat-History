import * as fs from "fs";
import { decoder } from "./converter/decoder";

function usage() {
  console.log("Usage:");
  console.log('  npx ts-node detokenize.ts <input.bas> <output.bas>');
  process.exit(2);
}

const args = process.argv.slice(2);
if (args.length < 2) usage();

const [inputPath, outputPath] = args;

const bytes = fs.readFileSync(inputPath);
const byteArray = Array.from(bytes.values()); // number[] for the decoder

const d = new decoder();
d.setStream(byteArray);

const text = d.decode();

fs.writeFileSync(outputPath, text, "utf8");
console.log(`Wrote ${outputPath}`);