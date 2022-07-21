const API_GITHUB_LIST_FILES = "https://api.github.com/repos/Haato3o/HunterPie-v2/git/trees/main?recursive=1"
const RAW_FILE = "https://raw.githubusercontent.com/Haato3o/HunterPie-v2/main/"
async function readXmlFile(path) {
    const req = await fetch(`${RAW_FILE}${path}`);
    const res = await req.text();
    const parser = new DOMParser();

    return parser.parseFromString(res, "text/xml");
}

async function getLocalizationFiles() {
    const table = document.getElementById("localization-table")

    if (table == null)
        return

    const DEFAULT_LOCALIZATION = "HunterPie/Languages/en-us.xml";
    const english = await readXmlFile(DEFAULT_LOCALIZATION)
    const flatEnglish = flatXml(english)

    const filesResponse = await fetch(API_GITHUB_LIST_FILES)
    const response = await filesResponse.json()
    const localizations = response.tree
        .filter(e => e.path.startsWith("HunterPie/Languages/"))
        .flatMap(e => e.path)

    const languages = []

    for (let i = 0; i < localizations.length; i++) {
        let file = localizations[i]
        let xmlFile = await readXmlFile(file)
        let flatXmlFile = flatXml(xmlFile)
        let percentage = compareStringsRecursively(flatEnglish, flatXmlFile)

        languages.push({
            "name": file,
            "percentage": percentage
        })
    }

    // Display results
    languages.forEach(data => {
        const tr = document.createElement("tr");
        const name = document.createElement("td");
        const percentage = document.createElement("td")

        tr.appendChild(name)
        tr.appendChild(percentage)

        name.innerHTML = data.name
            .replace("HunterPie/Languages/", "")
            .replace(".xml", "")

        percentage.innerHTML = `${(data.percentage * 100).toFixed(2)}%`

        const getGreenToRed = (percent) => {
            const g = 80 + 190 * percent;
            const b = 50 + 41 * percent;
            const r = 160 + (38 * (1 - percent));
            return `rgb(${r}, ${g}, ${b})`;
        }

        percentage.style.color = getGreenToRed(data.percentage)

        table.appendChild(tr)
    })
}

function flatXml(document) {
    const hashset = new Set()
    const elements = document.getElementsByTagName("*")
    for (let i = 0; i < elements.length; i++) {
        let key = getFullNodePath(elements[i])
        
        if (key === undefined)
            continue
        
        hashset.add(key)
    }
    return hashset
}

function getFullNodePath(node) {
    if (node.attributes.Id == undefined)
        return undefined

    let path = node.attributes.Id.value
    while (node.parentElement != null) {
        let parentName = node.parentElement.tagName
        path = `${parentName}.${path}`

        node = node.parentElement
    }
    return path
}

function compareStringsRecursively(original, localization) {
    const oLength = original.size;
    const missing = new Set()

    original.forEach(v => {
        if (!localization.has(v))
            missing.add(v)
    })
    return (oLength - missing.size) / oLength
}

document.addEventListener('DOMContentLoaded', (event) => {
    getLocalizationFiles()
});