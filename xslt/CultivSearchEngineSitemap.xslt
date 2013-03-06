<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
  <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:umbraco.contour="urn:umbraco.contour"
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets umbraco.contour ">

  <xsl:output method="xml" omit-xml-declaration="yes"/>

  <xsl:param name="currentPage"/>

  <xsl:variable name="urlPrefix">
    <xsl:text>http://</xsl:text>
    <xsl:value-of select="umbraco.library:RequestServerVariables('HTTP_HOST')" />
  </xsl:variable>

  <!-- update this variable on how deep your site map should be -->
  <xsl:variable name="maxLevelForSitemap" select="100"/>

  <xsl:template match="/">
    <!-- change the mimetype for the current page to xml -->
    <xsl:value-of select="umbraco.library:ChangeContentType('text/xml')"/>

    <xsl:call-template name="drawNodes">
      <xsl:with-param name="parent" select="$currentPage/ancestor-or-self::*[@level=1]"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="drawNodes">
    <xsl:param name="parent"/>
    <xsl:if test="umbraco.library:IsProtected($parent/@id, $parent/@path) = 0 
            or (umbraco.library:IsProtected($parent/@id, $parent/@path) = 1 and umbraco.library:IsLoggedOn() = 1)
            and @level &lt;= $maxLevelForSitemap ">

      <xsl:for-each select="$parent/* [@isDoc and string(umbracoNaviHide) != '1'] | $parent/node [string(./data [@alias='umbracoNaviHide']) != '1']">
        <!-- If the document does not have a template, nothing is shown in the frontend anyway.
        So this is not proper content and should not be in the sitemap -->
        <xsl:if test="@template &gt; 0">
          <url>
            <loc>
              <xsl:value-of select="$urlPrefix" />
              <xsl:value-of select="umbraco.library:NiceUrl(@id)" />
            </loc>
            <lastmod>
              <xsl:value-of select="umbraco.library:FormatDateTime(@updateDate, 'yyyy-MM-ddTHH:mm:ss+00:00')" />
            </lastmod>
            <xsl:if test="./data [@alias='searchEngineSitemapChangeFreq'] != '' or searchEngineSitemapChangeFreq != ''">
              <changefreq>
                <xsl:value-of select="./data [@alias='searchEngineSitemapChangeFreq'] | searchEngineSitemapChangeFreq" />
              </changefreq>
            </xsl:if>
            <xsl:if test="./data [@alias='searchEngineSitemapPriority'] != '' or searchEngineSitemapPriority != ''">
              <priority>
                <xsl:value-of select="./data [@alias='searchEngineSitemapPriority'] | searchEngineSitemapPriority" />
              </priority>
            </xsl:if>
          </url>
        </xsl:if>

        <xsl:if test="(count(./* [@isDoc and string(umbracoNaviHide) != '1' and @level &lt;= $maxLevelForSitemap]) &gt; 0) or (count(./node [string(./data [@alias='umbracoNaviHide']) != '1' and @level &lt;= $maxLevelForSitemap]) &gt; 0)">
          <xsl:call-template name="drawNodes">
            <xsl:with-param name="parent" select="."/>
          </xsl:call-template>
        </xsl:if>

      </xsl:for-each>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>