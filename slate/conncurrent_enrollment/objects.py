from datetime import datetime
from sql import runSql

class ApplicationCollection:
	def __init__(self, apps=[]):
		self.collection = {}
		for app in apps:
			self.insertApp( Application(app) )
	def isEmpty(self):
		return len(self.collection) == 0
	def insertApp(self, app):
		self.collection[ app.getId() ] = app
	def formatApps(self):
		for appId in self.collection:
			self.collection[appId].formatData()
	def removeOld(self, daysOld):
		newCollection = {}
		for appId in self.collection:
			if ( datetime.now() - self.collection[appId].getSubmittedDate() ).days < daysOld: 
				newCollection[appId] = self.collection[appId]
		self.collection = newCollection
	def dedup(self, other):
		# removes entries in self that are contained both in other and self
		newCollection = {}
		for appId in self.collection:
			if appId not in other.collection:
				newCollection[appId] = self.collection[appId]
		self.collection = newCollection
	def merge(self, other):
		# adds or updates entries in self given entries in other
		# note: other will overwrite what is in self
		self.collection.update(other.collection)
	def getAppsList(self):
		# returns a list of applications
		return [self.collection[appId].getData() for appId in self.collection]
	
class Application:
	def __init__(self, appData):
		self.appData = appData
		self.ID = appData["application_number"]
	def getId(self):
		return self.ID
	def getData(self):
		return self.appData
	def getSubmittedDate(self):
		return datetime.strptime(self.appData["student_signature_date"], "%Y-%m-%d")
	def getState(self):
		return self.appData["state"]
	def setState(self, state):
		self.appData["state"] = state
	def getHSName(self):
		return self.appData["high_school"]
	def getDistrictCode(self):
		return self.appData["district_id"]
	def getHSCode(self):
		return self.appData["high_school_id"]
	def formatData(self):
		self.abbreviateState()
		self.addCEEBCode()
	def abbreviateState(self):
		result, error = runSql("long_to_short_state.sql", self.getState())
		self.setState( result )
	def addCEEBCode(self):
		result, error = runSql("lea_to_ceeb.sql", self.getDistrictCode(), self.getHSCode())
		self.appData["high_school_ceeb"] = result
