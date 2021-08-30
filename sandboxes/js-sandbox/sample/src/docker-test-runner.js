const path = require("path");
const fs = require("fs");
const util = require("util");
const { exec } = require("child_process");

const execAsync = util.promisify(exec);
const readFileAsync = util.promisify(fs.readFile);

const testDir = path.resolve(__dirname);
const hasUnitTests = fs
  .readdirSync(testDir)
  .filter((f) => f.endsWith(".test.js"));

const runTests = () => {
  const commands = [];

  if (hasUnitTests) {
    commands.push(
      execAsync(
        "npx webpack --config webpack.unit-tests-config.js && node unit-tests.js"
      )
    );
  }

  return Promise.all(commands);
};

const read = (resultPath) => readFileAsync(resultPath).then(parse).catch(fail);

const parse = (buffer) => JSON.parse(buffer.toString());

const report = async () => {
  const unitCommand = hasUnitTests
    ? read(path.join(__dirname, "dist", "unit-tests-result.json"))
    : null;

  const testsResult = await unitCommand;
  let result = {
    verdict: "Ok",
    compilationOutput: "",
    output: "",
    error: "",
  };
  if (testsResult && testsResult.failures && testsResult.failures.length > 0) {
    const failure = testsResult.failures[0];
    result = {
      verdict: "Ok",
      compilationOutput: "",
      output: `${failure.fullTitle}: ${failure.err.message}`,
      error: "",
    };
  }
  console.info(JSON.stringify(result));
};

const fail = (err) => {
  console.error(err.stdout);
  process.exit(1);
};

runTests().then(report).catch(fail);
