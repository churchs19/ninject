var converter = new Showdown.converter();
function escapeWikiAnchors(text) {
	text = text.replace(/\[\[([^|\]]+)\|([^\]]+)\]\]/g, function(wholeMatch, m1, m2) {
		return "[" + m1 + "](" + (!(/\/\//).test(m2) ? '#' : '') + m2 + ")";
	});

	text = text.replace(/\[\[([^\]]+)\]\]/g, function(wholeMatch, m1) {
		return "[" + m1 + "](#" + m1.split(' ').join('-') + ")";
	});

	return text;
}

function showWikiPage(userName, repoName, pageName, extension, targetDivisionName) {
	var pageName = pageName || 'Home',
		wikiUrl = ['https://raw.github.com/wiki/', userName, '/', repoName, '/'].join(''),
		url = [
		"http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20data.headers%20where%20url%3D%22",
		wikiUrl, pageName, extension,
		"%22&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys"].join('');
	document.write(url);	
	var jqxhr = $.getJSON(url,
		function(data) {
			var content;
			
			if (data.query.results.resources.redirect) {
				content = "This is a non-existent page";
				pageName = "Not Found";
			} else {
				content = converter.makeHtml(escapeWikiAnchors(data.query.results.resources.content));
			}
			content = $(targetDivisionName).html(content);
			if (content.find('h1').length < 1) {
				content.prepend('<h1>' + pageName.replace('-',' ') + '</h1>');
			}        
		})
	.error(function() { alert("error"); })
}