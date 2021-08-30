const util = require('util')
const fs = require('fs')
const path = require('path')
const Mocha = require('mocha')

const readDirAsync = util.promisify(fs.readdir)
const writeFileAsync = util.promisify(fs.writeFile)

const testDir = path.resolve(__dirname, 'dist')

const runTests = async () => {
  const testFiles = (await readDirAsync(testDir)).filter(f =>
    f.endsWith('.test.js')
  )

  if (testFiles.length === 0) {
    return JSON.stringify({})
  }

  const mocha = new Mocha({
    ui: 'bdd',
    reporter: 'json',
  })

  testFiles.forEach(f => mocha.addFile(path.join(testDir, f)))

  const write = process.stdout.write
  process.stdout.write = () => {}

  return new Promise(resolve => {
    const runner = mocha.run(() => {
      process.stdout.write = write
      resolve(JSON.stringify(runner.testResults, null, 2))
    })
  })
}

runTests()
  .then(async res => {
    console.log('unit tests completed')
    await writeFileAsync(path.join(testDir, 'unit-tests-result.json'), res)
  })
  .catch(err => {
    console.error('unit tests failed')
    console.error(err)
    process.exit(1)
  })
