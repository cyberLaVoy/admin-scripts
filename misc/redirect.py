

#working example
"""
<rule name="domain/whats-next/parents-of-students" patternSyntax="ECMAScript" stopProcessing="true"><match url="^whats-next/parents-of-students(/)?$" /><action type="Redirect" url="https://domain/" appendQueryString="false" redirectType="Temporary" />
        <conditions logicalGrouping="MatchAny">
            <add input="{HTTP_HOST}" pattern="^domain$" />
        </conditions></rule>
"""

def formatRule(sections, target):
	name = sections[2]
	pattern = "^" + name + "$"
	match_url = "^"
	for i in range(3, len(sections)):
		name += ("/" + sections[i])
		match_url += (sections[i] + "/")
	
	match_url = match_url[0:-1]
	if match_url[-1] == "/":
		match_url = match_url[0:-1]
	match_url += "(/)?$"
	target_url = target
	
	rule = '<rule name="' + name + '" patternSyntax="ECMAScript" stopProcessing="true"><match url="' + match_url + '" /><action type="Redirect" url="' + target + '" appendQueryString="false" redirectType="Temporary" />'
	rule += "\n"
	rule += '\t<conditions logicalGrouping="MatchAny">'
	rule += "\n"
	rule += '\t\t <add input="{HTTP_HOST}" pattern="' + pattern + '" />'
	rule += "\n"
	rule += '\t</conditions>'
	rule += "\n"
	rule += '</rule>'

	print(rule)

def redirectRules(input_file):
	targets_log = []
	targets = []
	sections_list = []
	fin = open(input_file, "r")
	for line in fin:
		line = line.strip()
		urls = line.split()
		target = urls[1]
		sections = urls[0].split("/")
		targets.append(target)
		sections_list.append(sections)
	for i in range(0, len(targets)):
		sections = sections_list[i]
		target = targets[i]
		if (target not in targets_log):
			formatRule(sections, target)	
			targets_log.append(target)

def main():
	redirectRules("rules.txt")
main()


